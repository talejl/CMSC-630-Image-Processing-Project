#Import libraries
import os
import re
import numpy as np
from PIL import Image
from math import sqrt
from numba import njit
from pathlib import Path
from operator import eq
from click import style
from random import randrange

from typing import List, Callable, Optional


#Selection of color channel to convert to grayscale
def bgr_selection(grayscale_image_array: np.array, color: str = "red") -> np.array:
    if color == "red":
        return grayscale_image_array[:, :, 0]

    elif color == "green":
        return grayscale_image_array[:, :, 1]

    elif color == "blue":
        return grayscale_image_array[:, :, 2]
    
#Define the labels of the classes
labels: List[str] = ["cyl", "inter", "let", "mod", "para", "super", "svar"]

#Function to deserialize class labels
def deserialize_label(value: int) -> str:
    return labels[value]


#Function to serialize class labels
def serialize_label(label: str) -> int:
    return labels.index(label)


#Convert image to numpy array
def process_image(filename: Path) -> np.array:
    with Image.open(filename) as img:
        return np.array(img)

#Function to write features to CSV
def save_features(arr: np.array, filename: str) -> None:
    np.savetxt(filename, arr, delimiter=",")


#Function to load features from CSV
def load_features(filename: str) -> np.array:
    try:
        return np.loadtxt(filename, delimiter=",")
    except:
        return []

#Histogram generation function
@njit(fastmath=True)
def histogram(grayscale_image_array: np.array) -> np.array:
    # Initialize histogram with zeros
    hist: np.array = np.zeros(256)

    # Get size of array
    image_size: int = len(grayscale_image_array)

    for intensity in range(256):
        for i in range(image_size):

            # Loop through values and increment in histogram
            if grayscale_image_array.flat[i] == intensity:
                hist[intensity] += 1

    return hist


#Entropy calculation function
@njit
def entropy(img_array: np.array) -> int:

    marg = histogram(img_array) / img_array.size
    marg = np.array(list(filter(lambda p: p > 0, marg)))

    entropy = -np.sum(np.multiply(marg, np.log2(marg)))

    return entropy

#Accuracy calculation function
def accuracy_metric(actual, predicted):
    correct = list(map(eq, actual, predicted))

    return (sum(correct) / len(correct)) * 100.0


#Erosion function
def erode(img_array: np.array, win: int = 1) -> np.array:

    r = np.zeros_like(img_array)
    [yy, xx] = np.where(img_array > 0)
    offset = np.tile(range(-win, win + 1), (2 * win + 1, 1))
    x_offset = offset.flatten()
    y_offset = offset.T.flatten()

    n = len(xx.flatten())
    x_offset = np.tile(x_offset, (n, 1)).flatten()
    y_offset = np.tile(y_offset, (n, 1)).flatten()

    ind = np.sqrt(x_offset ** 2 + y_offset ** 2) > win
    x_offset[ind] = 0
    y_offset[ind] = 0

    xx = np.tile(xx, ((2 * win + 1) ** 2))
    yy = np.tile(yy, ((2 * win + 1) ** 2))

    nx = xx + x_offset
    ny = yy + y_offset

    #Ensure not out of bounds
    ny[ny < 0] = 0
    ny[ny > img_array.shape[0] - 1] = img_array.shape[0] - 1
    nx[nx < 0] = 0
    nx[nx > img_array.shape[1] - 1] = img_array.shape[1] - 1

    r[ny, nx] = 255

    return r.astype(np.uint8)


#Dilation function
def dilate(img_array: np.array, win: int = 3) -> np.array:

    inverted_img = np.invert(img_array)
    eroded_inverse = erode(inverted_img, win)
    eroded_img = np.invert(eroded_inverse)

    return eroded_img


#Funtion to determine the threshold to segment
def determine_threshold(hist: np.array, min_count: int = 5) -> int:

    num_bins = len(hist)
    hist_start = 0
    while hist[hist_start] < min_count:
        hist_start += 1 

    hist_end = num_bins - 1
    while hist[hist_end] < min_count:
        hist_end -= 1  

    histogram_center = int(
        round(np.average(np.linspace(0, 2 ** 8 - 1, num_bins), weights=hist))
    )
    left = np.sum(hist[hist_start:histogram_center])
    right = np.sum(hist[histogram_center : hist_end + 1])

    while hist_start < hist_end:
        if left > right:  
            left -= hist[hist_start]
            hist_start += 1
        else:  
            right -= hist[hist_end]
            hist_end -= 1
        new_center = int(
            round((hist_end + hist_start) / 2)
        ) 

        if new_center < histogram_center:  
            left -= hist[histogram_center]
            right += hist[histogram_center]
        elif new_center > histogram_center:
            left += hist[histogram_center]
            right -= hist[histogram_center]

        histogram_center = new_center

    return histogram_center


#Segmentation with histogram thresholding method
def histogram_thresholding(
    img_array: np.array, hist: Optional[np.array] = None
) -> np.array:

    if hist == None:
        hist = histogram(img_array)

    threshold = determine_threshold(hist)

    duplicate = img_array.copy()
    duplicate[duplicate > threshold] = 255
    duplicate[duplicate < threshold] = 0

    duplicate = duplicate.astype(np.uint8)

    return duplicate.reshape(img_array.shape)

#Opening function, uses erosion/dilation. Opening window is a custom parameter in the config
def opening(img_array: np.array, config: dict, hist: Optional[np.array] = None) -> np.array:
    segmented_img = histogram_thresholding(img_array)
    eroded = erode(segmented_img, config["OPENING_WINDOW"])
    opened = dilate(eroded, config["OPENING_WINDOW"])

    return opened

#Find the number of 0 intensity pixels
def area(img_array: np.array, config: dict, hist: Optional[np.array] = None) -> int:

    unique, counts = np.unique(img_array, return_counts=True)
    counter = dict(zip(unique, counts))

    zero_intensity_pxs = counter[0]

    return zero_intensity_pxs


#Function to calculate bound radition
@njit
def calculate_bound_radius(segmented_img: np.array) -> float:

    #Initialize with a center and radius
    center = np.array((0.0, 0.0))  

    radius = 0.0001 

    for _ in range(2):
        for pos, x in np.ndenumerate(segmented_img):

            array_position = np.array(pos)

            if x != 0:
                continue

            diff = array_position - center
            dist = np.sqrt(np.sum(diff ** 2))

            if dist < radius:
                continue

            alpha = dist / radius
            alphaSq = alpha ** 2

            radius = 0.5 * (alpha + 1.0 / alpha) * radius

            center = 0.5 * (
                (1.0 + 1.0 / alphaSq) * center + (1.0 - 1.0 / alphaSq) * array_position
            )

    for idx, _ in np.ndenumerate(segmented_img):

        array_position = np.array(idx)
        diff = array_position - center
        dist = np.sqrt(np.sum(diff ** 2))

        if dist < radius:
            break

        radius = (radius + dist) / 2.0
        center += (dist - radius) / dist * np.subtract(array_position, center)

    return radius


#Data normalization function
@njit
def normalize_data(dataset: np.array) -> np.array:

    normalized_data = dataset.copy()

    sans_labels = dataset[:, :-1]
    for idx, column in enumerate(sans_labels.T):
        smallest = np.min(column)
        largest = np.max(column)

        rng = largest - smallest

        if rng == 0:
            continue

        normalized_data[:, idx] = (normalized_data[:, idx] - smallest) / rng

    return normalized_data

#Perform feature extraction on a cancer cell image
def extract_features(config: dict, file: Path) -> dict:

    try:
        img = process_image(file)
        img = bgr_selection(img, config["COLOR_CHANNEL"])

        hist = histogram(img)
        opened = opening(img, config, hist)

        #Get class name
        search_obj = re.search(r"(\D+)(\d+).*", file.stem, re.M | re.I)
        label = search_obj.group(1)

        try:
            y = serialize_label(label)
        except KeyError:
            y = None

        number = search_obj.group(2)

        #Perform feature extraction
        
        # Calculate area of cluster
        F1 = area(opened, config, hist)

        # Calculate entropy of pixels,
        F2 = entropy(img)

        # Calculate histogram mean
        F3 = np.mean(hist)

        # Calculate radius of smallest enclosing sphere
        F4 = calculate_bound_radius(opened)

    #Error handling for corrupt image
    except Exception as e:
        return {
            "features": [],
            "msg": style(f"[ERROR] {file.stem} is corrupt: {e}", fg="red"),
        }
    #Dialog for user information
    return {
        "features": [F1, F2, F3, F4, y],
        "msg": style(f"{f'[{file.stem}]':15} Processed!", fg="green"),
    }

#Function to measure euclidean distance
def euclidean_distance(row1: np.array, row2: np.array) -> float:
    return np.linalg.norm(np.subtract(row1, row2))

#Function to find nearest neighbors 
def find_neighbors(train: np.array, testing_data: np.array, K: int) -> np.array:
    #Use euclidean distance to find the nearest neighbors
    distances = [(training_data, euclidean_distance(testing_data, training_data)) for training_data in train]
    distances.sort(key=lambda tup: tup[1])

    neighbors = np.array([distances[i][0] for i in range(K)])

    return neighbors

def knn(train: np.array, test: np.array, K: int) -> np.array:
    #KNN application 
    return np.array([predict_label(train, row, K) for row in test])


#Cross validation function
def cross_validation(dataset: np.array, n_folds: int) -> np.array:

    #Split up the data according to n folds. better than simple split 
    split_data = []
    replicated_data = dataset.copy()
    fold_size = len(dataset) // n_folds

    for _ in range(n_folds):
        fold = []

        while len(fold) < fold_size:
            index = randrange(len(replicated_data))
            fold.append(replicated_data[index])
            replicated_data = np.delete(replicated_data, index, axis=0)

        split_data.append(fold)

    return np.array(split_data)

#Measure how well the classifier performs.. unfortunately not well it seems
def evaluate_classifier(dataset: np.array, n_folds: int, K: int) -> List: 
    #Check KNN performance with N fold cross validation
    

    folds = cross_validation(dataset, n_folds)
    scores = []

    for idx, fold in enumerate(folds):
        training_data = np.delete(folds, idx, axis=0)
        training_data = np.concatenate(training_data, axis=0)
        testing_data = []

        for row in fold:
            row_copy = row.copy()
            testing_data.append(row_copy)
            row_copy[-1] = None

        testing_data = np.array(testing_data)

        predicted = knn(training_data, testing_data, K)
        actual = [row[-1] for row in fold]
        accuracy = accuracy_metric(actual, predicted)

        scores.append(accuracy)

    return scores

#Funciton to try and predict an image based on the knn model
def predict_label(train: np.array, test_row: np.array, K: int = 3) -> np.array:
    #Apply the KNN model to a specified image
    neighbors = find_neighbors(train, test_row, K)

    output_values = [row[-1] for row in neighbors]

    prediction = max(set(output_values), key=output_values.count)

    return prediction


