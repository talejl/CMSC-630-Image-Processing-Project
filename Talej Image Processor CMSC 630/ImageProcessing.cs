using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Talej_Image_Processor_CMSC_630.Properties;

namespace Talej_Image_Processor_CMSC_630
{
    //test comment
    public class ImageProcessing
    {
        #region Project Part 2
        #region Segmentation Methods
        public static Bitmap GlobalThresholding(Bitmap grayscaleImage)
        {
            //Initialize height and width
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //Calculate byte size
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create byte arrays
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Read information in 
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Computing initial threshold

            //Convert byte array to integer array
            int[] converted = originalImageBuffer.Select(x => (int)x).ToArray();

            //Divide sum of integer array by total bytes to determine initial threshold
            int threshold = converted.Sum() / byteSize;

            //Initialize Placeholder 
            int dThreshold = 1;

            //Exit loop when the delta is 0 
            while (dThreshold > 0)
            {

                //declare new histogram array
                int[] histogram = new int[256];

                //calculate teh histogram
                for (int i = 0; i < byteSize; i += 3)
                {
                    histogram[originalImageBuffer[i]]++;
                }

                //initialize placeholders for calculating threshold
                int mean1 = 0;
                int mean2 = 0;
                int sum1 = 0;
                int sum2 = 0;

                for (int i = 0; i < 256; i++)
                {
                    //calculate mean and sum below threshold
                    if (i <= threshold)
                    {
                        mean1 += histogram[i] * i;
                        sum1 += histogram[i];
                    }

                    //calculate mean and sum above threshold
                    else
                    {
                        mean2 += histogram[i] * i;
                        sum2 += histogram[i];
                    }
                }

                mean1 /= sum1;
                mean2 /= sum2;
                dThreshold = threshold;

                //Calculate threshold
                threshold = (mean1 + mean2) / 2;

                //set new delta
                dThreshold = Math.Abs(dThreshold - threshold);
            }

            //Apply the global threshold, will give a binary image
            for (int i = 0; i < byteSize; i++)
            {
                //Assign background 255, foreground as 0 for segmentation
                modifiedImageBuffer[i] = (byte)(originalImageBuffer[i] >= threshold ? 255 : 0);
            }


            //Write out global thresholded image
            Bitmap globalThresholdedImage = new Bitmap(width, height);
            BitmapData globalThresholdedImageData = globalThresholdedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, globalThresholdedImageData.Scan0, byteSize);
            globalThresholdedImage.UnlockBits(globalThresholdedImageData);

            return globalThresholdedImage;
        }
        public static Bitmap Superpixels(Bitmap grayscaleImage, int numberSuperPixels)
        {
            //Initialize width and height
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);

            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Calculate total number of pixels
            int numberTotalPixels = width * height;

            //Calculating sampling interval
            int samplingInterval = (int)Math.Floor(Math.Sqrt((double)numberTotalPixels / numberSuperPixels));


            //Create 
            int[][] means = new int[numberSuperPixels][];

            //Initialize placeholder superpixel value
            int superPixel = 0;

            //Calculate initial centers for the superpixels
            for (int y = samplingInterval / 2; y < height; y += samplingInterval)

            {
                for (int x = samplingInterval / 2; x < width; x += samplingInterval)

                {
                    //calculate position
                    int position =  y * grayscaleImageData.Stride + x * 3;

                    //compute lowest gradient
                    int lowestGradient = 99999;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int newPosition = position + i * 3 + j * grayscaleImageData.Stride;
                            int gradient = 0;
                            for (int k = -1; k <= 1; k++)
                            {
                                for (int l = -1; l <= 1; l++)
                                {
                                    int gradientPosition = newPosition + k * 3 + l * grayscaleImageData.Stride;

                                    //Apply sobel filter  to avoid centering on noise
                                    gradient += originalImageBuffer[gradientPosition] * ((int)Globals.xSobel[k + 1, l + 1] + (int)Globals.ySobel[k + 1, l + 1]);
                                }
                            }
                            if (lowestGradient > gradient)
                            {
                                lowestGradient = gradient;
                                means[superPixel] = new int[] { originalImageBuffer[newPosition], x + i, y + j };
                            }
                        }
                    }

                    modifiedImageBuffer[means[superPixel][1] * 3 + means[superPixel][2] * grayscaleImageData.Stride] = 255;
                    modifiedImageBuffer[means[superPixel][1] * 3 + means[superPixel][2] * grayscaleImageData.Stride + 1] = 255;
                    modifiedImageBuffer[means[superPixel][1] * 3 + means[superPixel][2] * grayscaleImageData.Stride + 2] = 255;

                    if (superPixel < numberSuperPixels - 1)
                    {
                        superPixel++;
                    }
                }
            }

            int[] labels = new int[byteSize];
            double[] distances = new double[byteSize];
            for (int i = 0; i < byteSize; i += 3)
            {
                labels[i] = -1;
                distances[i] = originalImageBuffer.Length;
            }

            double error = new double();
            double cc = 20;
            while (true)
            {
                int[][] newMeans = new int[numberSuperPixels][];

                //assign samples to clusters
                for (int i = 0; i < numberSuperPixels; i++)
                {
                    int m_pos = means[i][1] * 3 + means[i][2] * grayscaleImageData.Stride;
                    int xe = 2 * samplingInterval + means[i][1];
                    int xs = means[i][1] - 2 * samplingInterval;
                    int ye = 2 * samplingInterval + means[i][2];
                    int ys = means[i][2] - 2 * samplingInterval;

                    for (int x = (xs < 0 ? 0 : xs); x < ((xe < width) ? xe : width); x++)
                    {
                        for (int y = (ys < 0 ? 0 : ys); y < (ye < height ? ye : height); y++)
                        {
                            int position = x * 3 + y * grayscaleImageData.Stride;
                            double ds = Math.Sqrt(Math.Pow(x - means[i][1], 2) + Math.Pow(y - means[i][2], 2));
                            double dc = Math.Sqrt(Math.Pow(originalImageBuffer[position] - means[i][0], 2)
                                + Math.Pow(originalImageBuffer[position + 1] - means[i][0], 2)
                                + Math.Pow(originalImageBuffer[position + 2] - means[i][0], 2));
                            double distance = dc + (cc / samplingInterval) * ds;
                            if (distance < distances[position])
                            {
                                distances[position] = distance;
                                labels[position] = i;
                            }
                        }
                    }
                }

                //compute new means
                for (int i = 0; i < numberSuperPixels; i++)
                {
                    newMeans[i] = new int[3];
                    int samples = 0;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int position = x * 3 + y * grayscaleImageData.Stride;
                            if (labels[position] == i)
                            {
                                newMeans[i][0] += originalImageBuffer[position];
                                newMeans[i][1] += x;
                                newMeans[i][2] += y;
                                samples++;
                            }
                        }
                    }

                    for (int j = 0; j < 3; j++)
                    {
                        newMeans[i][j] /= samples;
                    }
                }

                //compute error
                double new_error = 0;
                for (int i = 0; i < numberSuperPixels; i++)
                {
                    new_error += (int)Math.Sqrt(Math.Pow(means[i][0] - newMeans[i][0], 2)
                        + Math.Pow(means[i][1] - newMeans[i][1], 2)
                        + Math.Pow(means[i][2] - newMeans[i][2], 2));
                    means[i] = newMeans[i];
                }

                if (error < new_error)
                {
                    break;
                }
                else
                {
                    error = new_error;
                }
            }


            //write out new values to modified image buffer
            for (int i = 0; i < numberSuperPixels; i++)
            {
                for (int j = 0; j < byteSize; j += 3)
                {
                    if (labels[j] == i)
                    {
                        modifiedImageBuffer[j] = (byte)means[i][0];
                        modifiedImageBuffer[j + 1] = (byte)means[i][0];
                        modifiedImageBuffer[j + 2] = (byte)means[i][0];
                    }
                }
            }


            //Write out superpixel segmented image
            Bitmap superPixelSegmentedImage = new Bitmap(width, height);
            BitmapData superPixelSegmentedImageData = superPixelSegmentedImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, superPixelSegmentedImageData.Scan0, byteSize);
            superPixelSegmentedImage.UnlockBits(superPixelSegmentedImageData);

            return superPixelSegmentedImage;
        }
        public static Bitmap OtsuThresholding(Bitmap grayscaleImage)
        {
            //Set height and width parameters
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);

            //Calculate size of byte arrays
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Calculate histogram
            double[] histogram = new double[256];

            //No need to loop through every byte since its grayscale
            for (int i = 0; i < byteSize; i += 3)
            {
                histogram[originalImageBuffer[i]]++;
            }

            //Histogram normalization
            histogram = histogram.Select(x => (x / (width * height))).ToArray();

            //calculate global mean from histogram
            double globalMean = 0;
            for (int i = 0; i < 255; i++)   //was 255
            {
                globalMean += i * histogram[i];
            }

            //Calculate variance between classe
            double interClassVariance = 0;

            //Initialize threshold placeholder
            int otsuThreshold = 0;

            for (int i = 0; i < 256; i++)
            {
                double cs = 0;
                double mean = 0;
                for (int j = 0; j < i; j++)
                {
                    cs += histogram[j];
                    mean += j * histogram[j];
                }

                if (cs != 0)
                {
                    double prevInterClassVariance = interClassVariance;
                    interClassVariance = Math.Max(interClassVariance, Math.Pow(globalMean * cs - mean, 2) / (cs * (1 - cs)));

                    if (interClassVariance > prevInterClassVariance)
                    {
                        //Set new threshold
                        otsuThreshold = i;
                    }
                }

            }

            //Write out segmented image
            for (int i = 0; i < byteSize; i++)
            {
                //write out segemented image based on threshold compared to original image byte value. Set to background to 255, foreground to 0
                modifiedImageBuffer[i] = (byte)((originalImageBuffer[i] > otsuThreshold) ? 255 : 0);
            }

            //Write segmented image back out
            Bitmap otsuSegmentedImage = new Bitmap(width, height);
            BitmapData otsuSegmentedImageData = otsuSegmentedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, otsuSegmentedImageData.Scan0, byteSize);
            otsuSegmentedImage.UnlockBits(otsuSegmentedImageData);

            return otsuSegmentedImage;
        }
        public static Bitmap KMeansSegmentation(Bitmap grayscaleImage, int numClusters)
        {
            //Set width and height
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Lock image into memory
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //Calculate size of byte array
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create byte arrays
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Store original information
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Create int array same size as number of specified numClusters to hold means
            int[] meanVals = new int[numClusters];

            //Intialize randomizer
            Random rnd = new Random();

            //Pick as many initial random means as specified by numClusters
            for (int i = 0; i < numClusters; i++)
            {
                //Need to avoid picking duplicate means
                int initialMean = rnd.Next(0, 255);
                while (meanVals.Contains((byte)initialMean))
                {
                    //Pick the mean as a random value between 0 and 255
                    initialMean = rnd.Next(0, 255);
                }

                //store mean byte values
                meanVals[i] = (byte)initialMean;
            }


            //Initialize error and samples
            double error = new double();
            List<byte>[] sampleIntensities = new List<byte>[numClusters];

            bool converge = false;
            //Perform kmeans
            while (converge == false)
            {
                //Create list of samples to assign intensities to clusters. Size is same as number of clusters
                for (int i = 0; i < numClusters; i++)
                {
                    sampleIntensities[i] = new List<byte>();
                }


                //loop for size of the image
                for (int i = 0; i < byteSize; i += 3)
                {
                    //stupid high number to guarantee we go into the assigning loop
                    double n = 999999;
                    int cluster = 0;

                    //loop for number of clusters
                    for (int j = 0; j < numClusters; j++)
                    {
                        double t = Math.Abs(originalImageBuffer[i] - meanVals[j]);
                        if (n > t)
                        {
                            n = t;
                            cluster = j;
                        }
                    }

                    //Assign value to the cluster from original image byte vals
                    sampleIntensities[cluster].Add(originalImageBuffer[i]);


                    //Write corresponding mean value to modified image buffer BGR 
                    modifiedImageBuffer[i] = (byte)meanVals[cluster];
                    modifiedImageBuffer[i+1] = (byte)meanVals[cluster];
                    modifiedImageBuffer[i+2] = (byte)meanVals[cluster];
                }


                //Begin check for convergence of means

                //Initialize new array of means same size as number of clusters
                int[] newMeanVals = new int[numClusters];

                //compute new means
                for (int i = 0; i < numClusters; i++)
                {
                    for (int j = 0; j < sampleIntensities[i].Count(); j++)
                    {
                        newMeanVals[i] += sampleIntensities[i][j];
                    }

                    newMeanVals[i] /= (sampleIntensities[i].Count() + 1);
                }

                //initialize new error
                int newError = 0;

                //set means and calculate new error
                for (int i = 0; i < numClusters; i++)
                {
                    newError += Math.Abs(meanVals[i] - newMeanVals[i]);
                    meanVals[i] = newMeanVals[i];
                }


                //If error doesn't change, we are done exit loop
                if (error == newError)
                {
                    converge = true;
                }
                //if error changes, repeat
                else
                {
                    //set error to new error
                    error = newError;
                }
            }



            //Write out modified image
            Bitmap kmeansSegmentedImage = new Bitmap(width, height);
            BitmapData kmeansSegmentedImageData = kmeansSegmentedImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, kmeansSegmentedImageData.Scan0, byteSize);
            kmeansSegmentedImage.UnlockBits(kmeansSegmentedImageData);

            return kmeansSegmentedImage;
        }
        #endregion
        #region Morphological Operators
        public static Bitmap Erosion(Bitmap grayscaleImage, byte [,] shape, int filterdim)
        {
            //set height and width parameters
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);
            grayscaleImage.UnlockBits(grayscaleImageData);

            //For 3x3 mask, filter size = 3 
            int filterSize = filterdim;

            //Calculate offset for mask to do work
            int filterOffset = (filterSize - 1) / 2;

            //Initialize calculation placeholders
            int erodedVal = 0;
            int position = 0;

            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    //Placeholder erosion intensity
                    byte intensity = 255;

                    //Calculate position
                    position = y * grayscaleImageData.Stride + x * 3;

                    //Apply erosion
                    for (int yfilter = -filterOffset; yfilter <= filterOffset; yfilter++)
                    {
                        for (int xfilter = -filterOffset; xfilter <= filterOffset; xfilter++)
                        {
                            //apply structuring element
                            if (shape[yfilter + filterOffset, xfilter + filterOffset] == 1)
                            {
                                erodedVal = position + yfilter * grayscaleImageData.Stride + xfilter * 3;
                                intensity = Math.Min(intensity, originalImageBuffer[erodedVal]);
                            }
                        }
                    }

                    //write to BGR channels
                    modifiedImageBuffer[position] = intensity;
                    modifiedImageBuffer[position + 1] = intensity;
                    modifiedImageBuffer[position + 2] = intensity;
                }
            }

            Bitmap erodedImage = new Bitmap(width, height);
            BitmapData resultData = erodedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, resultData.Scan0, byteSize);
            erodedImage.UnlockBits(resultData);
            return erodedImage;
        }
        public static Bitmap Dilation(Bitmap grayscaleImage, byte[,] shape, int filterdim)
        {
            //Set width and height 
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

           
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //calculate bytesize of the image arrays
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);
            grayscaleImage.UnlockBits(grayscaleImageData);

            int filterSize = filterdim;
            //This is the offset of center pixel from border of the filter
            int filterOffset = (filterSize - 1) / 2;
            int dilationVal = 0;
            int position = 0;
            for (int y = filterOffset; y < height - filterOffset; y++)
            {
                for (int x = filterOffset; x < width - filterOffset; x++)
                {
                    //Initial dilation placeholder intensity
                    byte intensity = 0;

                    //Calculate position
                    position = y * grayscaleImageData.Stride + x * 3;

                    //Apply dilation
                    for (int yfilter = -filterOffset; yfilter <= filterOffset; yfilter++)
                    {
                        for (int xfilter = -filterOffset; xfilter <= filterOffset; xfilter++)
                        {
                            //apply structuring element
                            if (shape[yfilter + filterOffset, xfilter + filterOffset] == 1)
                            {
                                dilationVal = position + yfilter * grayscaleImageData.Stride + xfilter * 3;
                                intensity = Math.Max(intensity, originalImageBuffer[dilationVal]);
                            }
                        }
                    }

                    //Write dilated value to BGR values
                    modifiedImageBuffer[position] = intensity;
                    modifiedImageBuffer[position + 1] = intensity;
                    modifiedImageBuffer[position + 2] = intensity;
                }
            }
            //Create output bitmap of this function
            Bitmap dilatedImage = new Bitmap(width, height);
            BitmapData dilatedImageData = dilatedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            //Write processed data into bitmap form
            Marshal.Copy(modifiedImageBuffer, 0, dilatedImageData.Scan0, byteSize);
            dilatedImage.UnlockBits(dilatedImageData);
            return dilatedImage;
        }

        #endregion
        #region Edge Detection

        public static Bitmap CompassEdgeDetection(Bitmap grayscaleImage)
        {
            //Initialize width and height
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Read image into memory
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);

            //Calculate size of byte array
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create byte arrays
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Read in information
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Import Kirsch filters
            double[][,] filters =
            {
             Globals.nKirsch,
             Globals.neKirsch,
             Globals.eKirsch,
             Globals.seKirsch,
             Globals.sKirsch,
             Globals.swKirsch,
             Globals.wKirsch,
             Globals.nwKirsch
         };

            //Apply Kirsch filters
            for (int z = 0; z < filters.Length; z++)
            {
                var filter = filters[z];
                byte[] filteredBytes = new byte[byteSize];
                //int xOffset = (filter.GetLength(0) - 1) / 2;  //Useful if I am doing any filter with dim other than 3. For this purpose I will assume offset = 1 because dim will always be 3
                //int yOffset = (filter.GetLength(1) - 1) / 2;
                int xOffset = 1;
                int yOffset = 1;

                for (int y = yOffset; y < grayscaleImageData.Height - yOffset; y++)
                {
                    for (int x = xOffset; x < grayscaleImageData.Width - xOffset; x++)

                    {
                        int position = (x * 3) + (y * grayscaleImageData.Stride);

                        //Initialize placeholder sum
                        int sum = 0;

                        //Apply filter
                        for (int i = -xOffset; i <= xOffset; i++)
                        {
                            for (int j = -yOffset; j <= yOffset; j++)
                            {
                                int filter_position = position + (i * 3) + (j * grayscaleImageData.Stride);
                                sum += (int)(originalImageBuffer[filter_position] * filter[i + xOffset, j + yOffset]);
                            }
                        }


                        //Store bounded applied results in temporary image buffer
                        filteredBytes[position] = (byte)Math.Min(255, Math.Abs(sum));
                        filteredBytes[position + 1] = (byte)Math.Min(255, Math.Abs(sum));
                        filteredBytes[position + 2] = (byte)Math.Min(255, Math.Abs(sum));
                    }
                }
                for (int i = 0; i < byteSize; i++)
                {
                    //Compare against blank image buffer, store larger result
                    modifiedImageBuffer[i] = Math.Max(modifiedImageBuffer[i], filteredBytes[i]);
                }
            }


            //Write out compass edge detected image
            Bitmap compassEdgeDetectedImage = new Bitmap(width, height);
            BitmapData compassEdgeDetectedImageData = compassEdgeDetectedImage.LockBits(new Rectangle(0, 0, width, height),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, compassEdgeDetectedImageData.Scan0, byteSize);
            compassEdgeDetectedImage.UnlockBits(compassEdgeDetectedImageData);

            return compassEdgeDetectedImage;
        }
        public static Bitmap LinearEdgeDetection(Bitmap grayscaleImage, double[,] xfilter, double[,] yfilter)
        {

            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];


            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);
            grayscaleImage.UnlockBits(grayscaleImageData);


            //Initialize variables for filter application 
            double xGray = 0.0;
            double yGray = 0.0;
            double tGray = 0.0;

            //Calculate filter offset when moving through the image so it isn't out of bounds
            // 3x3 = 1, 5x5 = 2
            int filterSize = 3;
            int filterOffset = (filterSize - 1) / 2; ///should be 1 when dim is 3

            //Initialize the offset vars
            int edgeCalcOffset = 0;
            int position = 0;

            
            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                    //reset values to 0
                    xGray = yGray = tGray = 0.0;

                    //Calculate position of the center pixel in the filter
                    position = OffsetY * grayscaleImageData.Stride + OffsetX * 3;


                    //Apply edge detection filter
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            //Calculate edge offset
                            edgeCalcOffset = position + filterX * 3 + filterY * grayscaleImageData.Stride;

                            //Apply x filter
                            xGray += (double)(originalImageBuffer[edgeCalcOffset]) * xfilter[filterY + filterOffset, filterX + filterOffset];

                            //Apply y filter
                            yGray += (double)(originalImageBuffer[edgeCalcOffset]) * yfilter[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    //Calculate Gray Total
                    tGray = Math.Sqrt((xGray * xGray) + (yGray * yGray));

                    //Bound values between 0 and 255
                    tGray = Math.Min(255, Math.Max(tGray, 0));

                    //Write out edge detected values in the modified image buffer to BGR
                    modifiedImageBuffer[position] = (byte)(tGray);
                    modifiedImageBuffer[position + 1] = (byte)(tGray);
                    modifiedImageBuffer[position + 2] = (byte)(tGray);
                }
            }


            //Write out edge detected image
            Bitmap edgeDetectedImage = new Bitmap(width, height);
            BitmapData edgeDetectedImageData = edgeDetectedImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, edgeDetectedImageData.Scan0, modifiedImageBuffer.Length);
            edgeDetectedImage.UnlockBits(edgeDetectedImageData);

            return edgeDetectedImage;
        }
        #endregion
        #endregion
        #region Project Part 1
        #region Filters     
        public static Bitmap MedianFilter5x5(Bitmap grayscaleImage, List<double> MaskWeights)
        {
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);


            int byteSize = grayscaleImageData.Stride * height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            grayscaleImage.UnlockBits(grayscaleImageData);
            int[] mask = new int[25];
            double r = 0;
            for (int i = 0; i < MaskWeights.Count; i++)
            {
                r = r + MaskWeights[i];
            }

            int[] weightedMask = new int[(int)r];


            for (int y = 0; y < height - 4; y++)
            {
                for (int x = 0; x < width - 4; x++)
                {
                    int count = 0;
                    for (int i = 0; i < (int)MaskWeights[0]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[0]);
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[1]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[1]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[2]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[2]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[3]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[3]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[4]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + +12] * (int)(MaskWeights[4]);
                        count++;
                    }



                    //next row
                    for (int i = 0; i < (int)MaskWeights[5]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[5]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[6]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[6]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[7]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[7]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[8]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[8]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[9]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[9]);
                        count++;
                    }



                    //Next row
                    for (int i = 0; i < (int)MaskWeights[10]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[10]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[11]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[11]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[12]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[12]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[13]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[13]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[14]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[14]);
                        count++;
                    }

                    //Next row
                    for (int i = 0; i < (int)MaskWeights[15]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[15]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[16]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[16]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[17]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[17]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[18]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[18]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[19]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[19]);
                        count++;
                    }

                    //next row
                    for (int i = 0; i < (int)MaskWeights[20]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[20]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[21]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[21]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[22]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[22]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[23]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[23]);
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[24]; i++)
                    {
                        weightedMask[count] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[24]);
                        count++;
                    }

                    //Sort the byte values. Complexity is O(nlog(n)) which is why this is a nonlinear process
                    Array.Sort(weightedMask);

                    //Median value will be in the middle of the array
                    int median = weightedMask[(int)(r / 2)];

                    //3 bytes in the column (RGB). We offset by 2 to get the middle row fo matrix
                    //Bytes are 0 thru 24 for 5x5 mask. 6,7,8 = middle byte offsets
                    //Replace middle pixel (3 byte values)
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6] = (byte)median;
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 7] = (byte)median;
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 8] = (byte)median;

                }
            }

            Bitmap medianImage = new Bitmap(width, height);
            BitmapData medianimgData = medianImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, medianimgData.Scan0, byteSize);
            medianImage.UnlockBits(medianimgData);
            return medianImage;
        }
        public static Bitmap LinearFilter5x5(Bitmap grayscaleImage, List<double> MaskWeights, double factor, double bias)
        {
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);


            int byteSize = grayscaleImageData.Stride * height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            grayscaleImage.UnlockBits(grayscaleImageData);
            int[] mask = new int[25];

            //bytes per pixel = 3 for 24bpp
            for (int y = 0; y < height - 4; y++)
            {
                for (int x = 0; x < width - 4; x++)
                {
                    mask[0] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[0]);
                    mask[1] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[1]);
                    mask[2] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[2]);
                    mask[3] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[3]);
                    mask[4] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + + 12] * (int)(MaskWeights[4]);


                    mask[5] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[5]);
                    mask[6] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[6]);
                    mask[7] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[7]);
                    mask[8] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[8]);
                    mask[9] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[9]);


                    mask[10] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[10]);
                    mask[11] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[11]);
                    mask[12] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[12]);
                    mask[13] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[13]);
                    mask[14] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[14]);

                    mask[15] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[15]);
                    mask[16] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[16]);
                    mask[17] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[17]);
                    mask[18] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[18]);
                    mask[19] = originalImageBuffer[(y + 3) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[19]);

                    mask[20] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 0] * (int)(MaskWeights[20]);
                    mask[21] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[21]);
                    mask[22] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[22]);
                    mask[23] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 9] * (int)(MaskWeights[23]);
                    mask[24] = originalImageBuffer[(y + 4) * grayscaleImageData.Stride + x * 3 + 12] * (int)(MaskWeights[24]);

                    int boundedSum = Math.Min(Math.Max((int)(factor * (mask[0] + mask[1] + mask[2] + mask[3] + mask[4] + mask[5] + mask[6] + mask[7] + mask[8] 
                                                                    + mask[9] + mask[10] + mask[11] + mask[12] + mask[13] + mask[14] + mask[15] + mask [16]
                                                                    + mask[17] + mask [18] + mask[19] + mask[20] + mask[21] + mask[22] + mask [23] + mask[24])+ bias), 0), 255);

                    //3 bytes in the column (RGB). We offset by 2 to get the middle row fo matrix
                    //Bytes are 0 thru 24 for 5x5 mask. 6,7,8 = middle byte offsets
                    //Replace middle pixel (3 byte values)
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6] = (byte)boundedSum;
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 7] = (byte)boundedSum;
                    modifiedImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 8] = (byte)boundedSum;

                }
            }

            Bitmap medianImage = new Bitmap(width, height);
            BitmapData medianimgData = medianImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, medianimgData.Scan0, byteSize);
            medianImage.UnlockBits(medianimgData);
            return medianImage;
        }
        public static Bitmap LinearFilter3x3(Bitmap grayscaleImage, List<double> MaskWeights, double bias, double factor)
        {
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int byteSize = grayscaleImageData.Stride * height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);


            grayscaleImage.UnlockBits(grayscaleImageData);

            int[] mask = new int[9];

            //bytes per pixel = 3 for 24bpp
            int k = 3;
            for (int y = 0; y < height - 2; y++)
            {
                for (int x = 0; x < width - 2; x++)
                {
                    mask[0] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * k] * (int)(MaskWeights[0]);

                    mask[1] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[1]);


                    mask[2] = originalImageBuffer[(y) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[2]);

                    mask[3] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * k] * (int)(MaskWeights[3]);

                    mask[4] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[4]);


                    mask[5] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[5]);


                    mask[6] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * k] * (int)(MaskWeights[6]);


                    mask[7] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 3] * (int)(MaskWeights[7]);



                    mask[8] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6] * (int)(MaskWeights[8]);


                    int boundedSum = Math.Min(Math.Max((int)(factor * mask[0] + mask[1] + mask[2] + mask[3] + mask[4] + mask[5] + mask[6] + mask[7] + mask[8] + bias), 0), 255);

                    //3 bytes in the column (RGB). We offset by 1 
                    //Bytes are 0 thru 8 for 3x3 mask. 0 through 2 are left pixel, 3 through 5 are middle pixel, 6 though 8 are rightpixel.
                    //Replace the middle byte values with the median
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3] = (byte)boundedSum;
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 4] = (byte)boundedSum;
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 5] = (byte)boundedSum;
                }
            }

            Bitmap medianImage = new Bitmap(width, height);
            BitmapData medianimgData = medianImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, medianimgData.Scan0, byteSize);
            medianImage.UnlockBits(medianimgData);
            return medianImage;
        }

        //Median filter
        public static Bitmap MedianFilter3x3(Bitmap grayscaleImage, List<double> MaskWeights)
        {
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int byteSize = grayscaleImageData.Stride * height;
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            grayscaleImage.UnlockBits(grayscaleImageData);
            int[] mask = new int[9];
            double r = 0;
            for (int i = 0; i < MaskWeights.Count; i++)
            {
                r = r + MaskWeights[i]; 
            }

            int[] weightedMask = new int[(int)r];

            //bytes per pixel = 3 for 24bpp
            int k = 3;
            for (int y = 0; y < height - 2; y++)
            {
                for (int x = 0; x < width - 2; x++)
                { 
                    int count = 0;
                    for (int i = 0; i < (int)MaskWeights[0]; i++ )
                    {
                        mask[0] = originalImageBuffer[y * grayscaleImageData.Stride + x * k];
                        weightedMask[count] = mask[0];
                        count++;
                    }
                    for (int i = 0; i < (int)MaskWeights[1]; i++)
                    {
                        mask[1] = originalImageBuffer[y * grayscaleImageData.Stride + x * 3 + 3];
                        weightedMask[count] = mask[1];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[2]; i++)
                    {
                        mask[2] = originalImageBuffer[y * grayscaleImageData.Stride + x * 3 + 6];
                        weightedMask[count] = mask[2];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[3]; i++)
                    {
                        mask[3] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * k];
                        weightedMask[count] = mask[3];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[4]; i++)
                    {
                        mask[4] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3];
                        weightedMask[count] = mask[4];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[5]; i++)
                    {
                        mask[5] = originalImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 6];
                        weightedMask[count] = mask[5];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[6]; i++)
                    {
                        mask[6] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * k];
                        weightedMask[count] = mask[6];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[7]; i++)
                    {
                        mask[7] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 3];
                        weightedMask[count] = mask[7];
                        count++;
                    }

                    for (int i = 0; i < (int)MaskWeights[8]; i++)
                    {
                        mask[8] = originalImageBuffer[(y + 2) * grayscaleImageData.Stride + x * 3 + 6];
                        weightedMask[count] = mask[8];
                        count++;
                    }
                    
                    //Sort the byte values. Complexity is O(nlog(n)) which is why this is a nonlinear process
                    Array.Sort(weightedMask);

                    //Median value will be in the middle of the array
                    int median = weightedMask[(int)(r/2)];


                    //3 bytes in the column (RGB). We offset by 1 
                    //Bytes are 0 thru 8 for 3x3 mask. 0 through 2 are left, 3 through 5 are middle, 6 though 8 are right.
                    //Replace the middle byte values with the median
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 3] = (byte)median;
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 4] = (byte)median;
                    modifiedImageBuffer[(y + 1) * grayscaleImageData.Stride + x * 3 + 5] = (byte)median;


                }
            }

            Bitmap medianImage = new Bitmap(width, height);
            BitmapData medianimgData = medianImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, medianimgData.Scan0, byteSize);
            medianImage.UnlockBits(medianimgData);
            return medianImage;
        }

        //Deprecated slower linear 5x5 filter
        //public static Bitmap Linear5x5(Bitmap image, List<double> MaskWeights, double factor, double bias)
        //{
        //    if (image != null)
        //    {
        //        Bitmap sharpenImage = (Bitmap)image.Clone();

        //        int filterWidth = 5;
        //        int filterHeight = 5;
        //        int width = image.Width;
        //        int height = image.Height;

        //        // Create 5x5 Linear Filter
        //        double[,] filter = new double[filterWidth, filterHeight];
        //        filter[0, 0] = MaskWeights[0];
        //        filter[0, 1] = MaskWeights[1];
        //        filter[0, 2] = MaskWeights[2];
        //        filter[0, 3] = MaskWeights[3];
        //        filter[0, 4] = MaskWeights[4];
        //        filter[1, 0] = MaskWeights[5];
        //        filter[1, 1] = MaskWeights[6];
        //        filter[1, 2] = MaskWeights[7];
        //        filter[1, 3] = MaskWeights[8];
        //        filter[1, 4] = MaskWeights[9];
        //        filter[2, 0] = MaskWeights[10];
        //        filter[2, 1] = MaskWeights[11];
        //        filter[2, 2] = MaskWeights[12];
        //        filter[2, 3] = MaskWeights[13];
        //        filter[2, 4] = MaskWeights[14];
        //        filter[2, 0] = MaskWeights[15];
        //        filter[3, 1] = MaskWeights[16];
        //        filter[3, 2] = MaskWeights[17];
        //        filter[3, 3] = MaskWeights[18];
        //        filter[3, 4] = MaskWeights[19];
        //        filter[3, 0] = MaskWeights[20];
        //        filter[4, 1] = MaskWeights[21];
        //        filter[4, 2] = MaskWeights[22];
        //        filter[4, 3] = MaskWeights[23];
        //        filter[4, 4] = MaskWeights[24];
               

        //        //double factor = 1.0;
        //        //double bias = 0.0;

        //        // Lock image bits for read/write.
        //        BitmapData imageData = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //        // Declare an array to hold the bytes of the bitmap.
        //        int bytes = Math.Abs(imageData.Stride) * height;
        //        byte[] originalImageBuffer = new byte[bytes];
        //        byte[] modifiedImageBuffer = new byte[bytes];


        //        // Copy the RGB values into the array.
        //        Marshal.Copy(imageData.Scan0, originalImageBuffer, 0, bytes);
        //        sharpenImage.UnlockBits(imageData);

        //        int position;

        //        for (int y = 0; y < height; y++)
        //        {
        //            //loop through horizontal pixels
        //            for (int x = 0; x < width; x++)
        //            {
        //                double filteredByte = 0.0;

        //                for (int filterY = 0; filterY < filterHeight; filterY++)
        //                {
        //                    for (int filterX = 0; filterX < filterWidth; filterX++)
        //                    {
        //                        int imageX = (x - filterWidth / 2 + filterX + width) % width;
        //                        int imageY = (y - filterHeight / 2 + filterY + height) % height;

        //                        position = (imageY * imageData.Stride) + (3 * imageX);


        //                        filteredByte += originalImageBuffer[position] * filter[filterX, filterY];
        //                    }

        //                    int bytee = Math.Min(Math.Max((int)(factor * filteredByte + bias), 0), 255);

        //                    position = (y * imageData.Stride) + (3 * x);
        //                    for (int channel = 0; channel < 3; channel++)
        //                    {
        //                        modifiedImageBuffer[position + channel] = (byte)bytee;
        //                    }
        //                }
        //            }
        //        }


        //        Bitmap sharpenedImage = new Bitmap(width, height);
        //        BitmapData shpimgData = sharpenedImage.LockBits(new Rectangle(0, 0, width, height),
        //            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        //        Marshal.Copy(modifiedImageBuffer, 0, shpimgData.Scan0, bytes);
        //        sharpenedImage.UnlockBits(shpimgData);
        //        return sharpenedImage;

        //    }
        //    else
        //    {
        //        return null;

        //    }
        //}


        //Deprecated slower linear 3x3 filter
        //public static Bitmap Linear3x3(Bitmap image, List<double>MaskWeights, double factor, double bias)
        //{
        //    if (image != null)
        //    {
        //        Bitmap sharpenImage = (Bitmap)image.Clone();

        //        int filterWidth = 3;
        //        int filterHeight = 3;
        //        int width = image.Width;
        //        int height = image.Height;

        //        // Create sharpening filter.
        //        double[,] filter = new double[filterWidth, filterHeight];
        //        filter[0, 0] = MaskWeights[0];
        //        filter[0, 1] = MaskWeights[1];
        //        filter[0, 2] = MaskWeights[2];
        //        filter[1, 0] = MaskWeights[3];
        //        filter[1, 1] = MaskWeights[4];
        //        filter[1, 2] = MaskWeights[5];
        //        filter[2, 0] = MaskWeights[6];
        //        filter[2, 1] = MaskWeights[7];
        //        filter[2, 2] = MaskWeights[8];

        //        //double factor = 1.0;
        //        //double bias = 0.0;

        //        // Lock image bits for read/write.
        //        BitmapData imageData = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        //        // Declare an array to hold the bytes of the bitmap.
        //        int bytes = Math.Abs(imageData.Stride) * height;
        //        byte[] originalImageBuffer = new byte[bytes];
        //        byte[] modifiedImageBuffer = new byte[bytes];


        //        // Copy the RGB values into the array.
        //        Marshal.Copy(imageData.Scan0, originalImageBuffer, 0, bytes);
        //        sharpenImage.UnlockBits(imageData);

        //        int position;

        //        for (int y = 0; y < height; y++)
        //        {
        //            //loop through horizontal pixels
        //            for (int x = 0; x < width; x++)
        //            {
        //                double filteredByte = 0.0;

        //                for (int filterY = 0; filterY < filterHeight; filterY++)
        //                {
        //                    for (int filterX = 0; filterX < filterWidth; filterX++)
        //                    {
        //                        int imageX = (x - filterWidth / 2 + filterX + width) % width;
        //                        int imageY = (y - filterHeight / 2 + filterY + height) % height;

        //                        position = (imageY * imageData.Stride) + (3 * imageX);


        //                        filteredByte += originalImageBuffer[position] * filter[filterX, filterY];
        //                    }

        //                    int bytee = Math.Min(Math.Max((int)(factor * filteredByte + bias), 0), 255);

        //                    position = (y * imageData.Stride) + (3 * x);
        //                    for (int channel = 0; channel < 3; channel++)
        //                    {
        //                        modifiedImageBuffer[position + channel] = (byte) bytee;
        //                    }
        //                }
        //            }
        //        }


        //        Bitmap sharpenedImage = new Bitmap(width, height);
        //        BitmapData shpimgData = sharpenedImage.LockBits(new Rectangle(0, 0, width, height),
        //            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        //        Marshal.Copy(modifiedImageBuffer, 0, shpimgData.Scan0, bytes);
        //        sharpenedImage.UnlockBits(shpimgData);
        //        return sharpenedImage;

        //    }
        //    else
        //    {
        //        return null;

        //    }
        //}

        #endregion
        #region Image Quantization Methods


        public static Bitmap UniformImageQuantization(Bitmap grayscaleImage, int level, string filename)
        {

            //Get width and height of the source image
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Get bitmap data from source image
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            //Calculate size of the image
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create new byte arrays the size of the image
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Copy in the source image to the byte array
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Get total number of pixels
            int totalPixels = width * height;

            //User defined quantization threshold
            int thresholds = level;

            //Calculate the step size for the desired threshold level
            //For 8 colors, should be 256/ 8 = 32 steps
            int stepSize = (int)Math.Ceiling(256.0 / thresholds);

            //New quantized intensities
            int[] quantizedIntensities = new int[256];

            //quantization error placeholder
            double quantizationError = 0.0;

            //calculate quantized intensities as per user specified 
            //Test 8 levels, that means we will execute the outer loop 8 times
            for (int i = 0; i < thresholds; i++)
            {
                //The step size for 8 levels is 32, so this inner loop will execute 32 times
                for (int j = (i * stepSize); j < Math.Min(255, ((i + 1) * stepSize)); j++)
                {
                    //The min will be the index multiplied by the determined step size
                    double min = i * stepSize;

                    //the max will be the next upcoming index multiplied by the step size, capped at 255 for maximum intensity
                    double max = Math.Min(255, (i + 1) * stepSize);

                    //Determine the new quantized intensity in that step
                    quantizedIntensities[j] = (int)(((max + min) / 2));

                }
            }

            //Write out new image based on quantized intensities
            //loop vertical pixels
            for (int y = 0; y < height; y++)
            {
                //loop through horizontal pixels
                for (int x = 0; x < width; x++)
                {
                    //Get pixel position 
                    int position = (y * grayscaleImageData.Stride) + (x * 3);

                    //Loop through the 3 color channels
                    for (int channel = 0; channel < 3; channel++)
                    {
                        //Running total of the quantization error on a per intensity basis
                        quantizationError += Math.Pow(quantizedIntensities[originalImageBuffer[position + channel]] - originalImageBuffer[position + channel], 2);

                        //Use value of original intensity to apply intensity in the quantized intensity at that index and write it out ot the modified image
                        modifiedImageBuffer[position + channel] = (byte)quantizedIntensities[originalImageBuffer[position + channel]];

                    }
                }
            }

            //calculate quantization error by dividing total error by total pixels
            quantizationError = quantizationError / totalPixels;
            if(filename != null)
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.Write(quantizationError);
                }
            }

            Bitmap quantizedImage = new Bitmap(width, height);
            BitmapData quantizedImageData = quantizedImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, quantizedImageData.Scan0, byteSize);
            quantizedImage.UnlockBits(quantizedImageData);
            return quantizedImage;
        }

        public static double CalculateUniformImageQuantizationError(Bitmap grayscaleImage, int level)
        {

            //Get width and height of the source image
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Get bitmap data from source image
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            //Calculate size of the image
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create new byte arrays the size of the image
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Copy in the source image to the byte array
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Get total number of pixels
            int totalPixels = width * height;

            //User defined quantization threshold
            int thresholds = level;

            //Calculate the step size for the desired threshold level
            //For 8 colors, should be 256/ 8 = 32 steps
            int stepSize = (int)Math.Ceiling(256.0 / thresholds);

            //New quantized intensities
            int[] quantizedIntensities = new int[256];

            //quantization error placeholder
            double quantizationError = 0.0;

            //calculate quantized intensities as per user specified 
            //Test 8 levels, that means we will execute the outer loop 8 times
            for (int i = 0; i < thresholds; i++)
            {
                //The step size for 8 levels is 32, so this inner loop will execute 32 times
                for (int j = (i * stepSize); j < Math.Min(255, ((i + 1) * stepSize)); j++)
                {
                    //The min will be the index multiplied by the determined step size
                    double min = i * stepSize;

                    //the max will be the next upcoming index multiplied by the step size, capped at 255 for maximum intensity
                    double max = Math.Min(255, (i + 1) * stepSize);

                    //Determine the new quantized intensity in that step by averaging the max and min
                    quantizedIntensities[j] = (int)(((max + min) / 2));

                }
            }

            //Write out new image based on quantized intensities
            //loop vertical pixels
            for (int y = 0; y < height; y++)
            {
                //loop through horizontal pixels
                for (int x = 0; x < width; x++)
                {
                    //Get pixel position 
                    int position = (y * grayscaleImageData.Stride) + (x * 3);

                    //Loop through the 3 color channels
                    for (int channel = 0; channel < 3; channel++)
                    {
                        //Running total of the quantization error on a per intensity basis
                        quantizationError += Math.Pow(quantizedIntensities[originalImageBuffer[position + channel]] - originalImageBuffer[position + channel], 2);

                        //Use value of original intensity to apply intensity in the quantized intensity at that index and write it out ot the modified image
                        modifiedImageBuffer[position + channel] = (byte)quantizedIntensities[originalImageBuffer[position + channel]];

                    }
                }
            }

            //calculate quantization error by dividing total error by total pixels
            quantizationError = quantizationError / totalPixels;

            Bitmap quantizedImage = new Bitmap(width, height);
            BitmapData quantizedImageData = quantizedImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, quantizedImageData.Scan0, byteSize);
            quantizedImage.UnlockBits(quantizedImageData);
            return quantizationError;
        }

        //Deprecated method, does not work as expected
        //public static Bitmap ImageQuantization(Bitmap sourceBitmap, int level)
        //{
        //    int number = level;

        //    Bitmap bm = sourceBitmap;
        //    //Get width and height of the source image
        //    int width = bm.Width;
        //    int height = bm.Height;

        //    //Get bitmap data from source image
        //    BitmapData imageData = bm.LockBits(new Rectangle(0, 0, width, height),
        //        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //    //Calculate size of the image
        //    int bytes = imageData.Stride * imageData.Height;

        //    //Create new byte arrays the size of the image
        //    byte[] originalImageBuffer = new byte[bytes];
        //    byte[] modifiedImageBuffer = new byte[bytes];
        //    //Copy in the source image to the byte array
        //    Marshal.Copy(imageData.Scan0, originalImageBuffer, 0, bytes);

        //    //Release image from memory
        //    bm.UnlockBits(imageData);

        //    Dictionary<int, int> hist = new Dictionary<int, int>();

        //    for (int y = 0; y < height; y++)
        //    {
        //        //loop through horizontal pixels
        //        for (int x = 0; x < width; x++)
        //        {
        //            //Get pixel position byte value
        //            int position = (y * imageData.Stride) + (x * 3);

        //            for (int channel = 0; channel < 3; channel++)
        //            {
        //                int i = originalImageBuffer[position + channel];
        //                if(hist.ContainsKey(i))
        //                {
        //                    hist[i]++;
        //                }
        //                else
        //                {
        //                    hist.Add(i, 1);
        //                }
        //            }
        //        }
        //    }
        //    var result0 = hist.OrderByDescending(a => a.Value);

        //    var mostusedbyte = result0.Select(x => x.Key).Take(number).ToList();

        //    Double temp2;
        //    Dictionary<int, double> dist2 = new Dictionary<int, double>();
        //    Dictionary<int, int> mapping2 = new Dictionary<int, int>();
        //    foreach (var b in result0)
        //    {
        //        dist2.Clear();
        //        foreach (int bb in mostusedbyte)
        //        {
        //            temp2 = Math.Abs(b.Key - bb);
        //            dist2.Add(bb, temp2);
        //        }
        //        var min2 = dist2.OrderBy(k => k.Value).FirstOrDefault();
        //        mapping2.Add(b.Key, min2.Key);
        //    }

        //    //loop through vertical pixels
        //    for (int y = 0; y < height; y++)
        //    {
        //        //loop through horizontal pixels
        //        for (int x = 0; x < width; x++)
        //        {
        //            //Get pixel position 
        //            int position = (y * imageData.Stride) + (x * 3);

        //            //Loop through the 3 color channels
        //            for (int channel = 0; channel < 3; channel++)
        //            {
        //                //Match and replace each of the original colors with the new palette
        //                modifiedImageBuffer[position + channel] = (byte)(mapping2[originalImageBuffer[position+channel]]);
        //            }
        //        }
        //    }

        //    Bitmap quantizedImage = new Bitmap(width, height);
        //    BitmapData eqimgData = quantizedImage.LockBits(new Rectangle(0, 0, width, height),
        //        ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        //    Marshal.Copy(modifiedImageBuffer, 0, eqimgData.Scan0, bytes);
        //    quantizedImage.UnlockBits(eqimgData);
        //    return quantizedImage;
        //}
        #endregion
        #region Histogram Operation Methods

        //Deprecated method, yields completely noisy results but good contrast.
        public static Bitmap HistogramEqualizedImageMethod2(Bitmap grayscaleImage)
        {

            //Get width and height of the source image
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Declare new color palette for new intensities
            int[] newColorPalette = new int[256];

            //Get total pixels
            int totalpixels = width * height;

            //histogram placeholder
            double[] p = new double[256];

            //intensity sum placeholder
            double inProbSum = 0;

            //for new histogram
            int[] newBins = new int[256];

            //Get bitmap data from source image
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //Calculate bytesize of the image
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create new byte arrays the size of the image
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Copy in the source image to the byte array
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Declare new array for equalized histogram values
            double[] eqHist = new double[256];

            //zero out histogram and palette
            for (int i = 0; i < 256; i++)
            {
                newBins[i] = 0;
                newColorPalette[i] = 0;
            }

            //loop through the original image buffer and count the byte values to make the histogram array
            //Byte incrementation is 3 for 24bpp. The colors will be same in all 3 bytes post grayscale conversion
            for (int i = 0; i < byteSize; i += 3)
            {
                eqHist[originalImageBuffer[i]]++;
            }

            for (int i = 0; i < 256; i++)
            {
                //Probability of this intensity occuring in the image. divide count of intensity by total pixels
                p[i] = (double)eqHist[i] / (double)totalpixels;

                newBins[(int)Math.Floor(255 * inProbSum)] += (int)eqHist[i];
                newColorPalette[i] = (int)Math.Floor(255 * inProbSum);

                //The intensity sum should be 1 after adding all the probabilities
                inProbSum += p[i];
            }


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Get pixel position byte value
                    int position = (y * grayscaleImageData.Stride) + (x * 3);

                    //Initialize placeholder for coefficient 
                    double sum = 0;

                    //Loop through all the bytes of this original pixel and tally them up
                    for (int i = 0; i < originalImageBuffer[position]; i++)
                    {
                        //sum the new coefficient from the intensity probablility histogram 
                        sum += newColorPalette[i];
                    }

                    for (int channel = 0; channel < 3; channel++)
                    {
                        //Apply new color palette
                        modifiedImageBuffer[position + channel] = (byte)sum;
                    }
                }
            }

            //Create modified image
            Bitmap equalizedImage = new Bitmap(width, height);
            BitmapData eqimgData = equalizedImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, eqimgData.Scan0, byteSize);
            equalizedImage.UnlockBits(eqimgData);
            //var histogramEqImageName = Path.Combine(Settings.Default.OutputPath, "testequal.bmp");

            //equalizedImage.Save(histogramEqImageName);
            return equalizedImage;
        }

        public static Bitmap HistogramEqualizedImage(Bitmap grayscaleImage)
        {
            //Get width and height of the source image
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            int totalPixels = width * height;
            //Get bitmap data from source image
            BitmapData grayscaleImageData = grayscaleImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            //Calculate bytesize of the image
            int byteSize = grayscaleImageData.Stride * grayscaleImageData.Height;

            //Create new byte arrays the size of the image
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Copy in the source image to the byte array
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);

            //Declare new array for equalized histogram values
            double[] eqHist = new double[256];

            //loop through the original image buffer and count the byte values to make the histogram array
            //Byte incrementation is 3 for 24bpp, the 3 byte values will be the same post grayscale conversion
            for (int i = 0; i < byteSize; i +=3)
            {
                eqHist[originalImageBuffer[i]]++;
            }

            //Calculate the frequency of the value occuring in the image
            for (int freqIntensity = 0; freqIntensity < eqHist.Length; freqIntensity++)
            {
                //Take the number of intensity occurences and divide by the total number of pixels to get the coefficients
                eqHist[freqIntensity] /= totalPixels;

            }
            //For debug to test and make sure the sum of the histogram array is 1
            ////double ratioHist = eqHist.Sum();

            for (int x = 0; x < width; x++)
            {
                //loop through horizontal pixels
                for (int y = 0; y < height; y++)
                {
                    //Get pixel position byte value
                    int position = (y * grayscaleImageData.Stride) + (x * 3);

                    //Initialize placeholder for coefficient 
                    double sum = 0;

                    //Loop through all the bytes of this original pixel and tally them up
                    for (int i = 0; i < originalImageBuffer[position]; i++)
                    {
                        //sum corresponding coefficient from byte value
                        sum += eqHist[i];
                    }
                    for (int channel = 0; channel < 3; channel++)
                    {
                        //Write new value with applied coefficient to each color channel of the pixel in the modified image buffer
                        //We apply the coefficient to the maximum intensity which is 255
                        modifiedImageBuffer[position + channel] = (byte)Math.Floor(255 * sum);
                    }
                }
            }

            //Create modified image
            Bitmap equalizedImage = new Bitmap(width, height);
            BitmapData eqgrayscalegrayscaleImageData = equalizedImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, eqgrayscalegrayscaleImageData.Scan0, byteSize);
            equalizedImage.UnlockBits(eqgrayscalegrayscaleImageData);
            return equalizedImage;
        }

        public static int[] CalculateHistogram(Bitmap grayScaleImage)
        {
            var histogram = new int[256];

            //24bit image = 1 pixel value per 3 bytes


            //Image handling based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=dotnet-plat-ext-6.0
            //Feed in the bitmap image

            //Get width and height from bitmap. In the case of the cancer cells, the images are uniform (768 px width, 568 px height)
            int width = grayScaleImage.Width;
            int height = grayScaleImage.Height;

            //Lock dimensions of image into memory
            BitmapData grayscaleImageData = grayScaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            //Calculate size of the image 
            //int bytes = image_data.Stride * image_data.Height;
            int byteSize = Math.Abs(grayscaleImageData.Stride) * grayscaleImageData.Height;

            //Create new byte arrays of the same size
            byte[] grayImageBuffer = new byte[byteSize];

            //Copy the original image into buffer as suggested by Dr. Krawczyk...bad things happen if we modify the original image as we read from it. 
            Marshal.Copy(grayscaleImageData.Scan0, grayImageBuffer, 0, byteSize);

            //Release original image from memory
            grayScaleImage.UnlockBits(grayscaleImageData);


            byte intensity = 0;
            //Grayscale grade series clear zero
            //Calculate the number of pixels of each grayscale
            //Theoretically it should be incremented by one, but because for whatever reason it keeps the image as 24bpp, we have to increment by 3

            for (int i = 0; i < byteSize; i ++)
            {
                //store the intensity value of the byte
                intensity = grayImageBuffer[i];

                //Add the intensity to the histogram
                histogram[intensity]++;
            }

            return histogram;
        }



        public static void GenerateCellClassAvgHistogram(string cellClass)
        {
            string inputPath = Settings.Default["InputPath"].ToString();
            string outputPath = Settings.Default["OutputPath"].ToString();
            var Files = new DirectoryInfo(inputPath)
            .GetFiles()
            .Where(f => f.IsImage() && f.Name.Contains(cellClass));
            if (Files.Count() > 0)
            {
                int[] ClassHistArray = new int[256];

                foreach (var file in Files)
                {
                    Bitmap image = (Bitmap)Bitmap.FromFile(file.FullName);
                    Bitmap grayImage = RGB2GrayscaleImage(image, Settings.Default.ColorChoice);
                    int[] histogramArray = CalculateHistogram(grayImage);
                    ClassHistArray = AddArrays(ClassHistArray, histogramArray);
                }

                float[] AvgClassHistArray = new float[256];
                for (int i = 0; i < AvgClassHistArray.Length; i++)
                {
                    AvgClassHistArray[i] = ClassHistArray[i] / Files.Count();

                }
                float maxIntensity = 0;

                for (int i = 0; i < AvgClassHistArray.Length; i++)
                {
                    if (AvgClassHistArray[i] > maxIntensity)
                    {
                        //Set the new highest intensity value 
                        maxIntensity = AvgClassHistArray[i];
                    }
                }

                int histHeight = 288;
                Bitmap img = new Bitmap(256, histHeight + 10);
                using (Graphics g = Graphics.FromImage(img))
                {
                    //loop through all intensity values in the histogram array
                    for (int i = 0; i < AvgClassHistArray.Length; i++)
                    {
                        float pct = AvgClassHistArray[i] / maxIntensity;   //get ratio of given histogram intensity element compared to the maximum recorded intesnity
                        g.DrawLine(Pens.Black,
                            new Point(i, img.Height - 5),
                            new Point(i, img.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                            );
                    }
                }
                string histogramImageName = "";
                switch (cellClass)
                {
                    case "cyl":
                        histogramImageName = Path.Combine(outputPath, "cyl_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "inter":
                        histogramImageName = Path.Combine(outputPath, "inter_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "para":
                        histogramImageName = Path.Combine(outputPath, "para_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "let":
                        histogramImageName = Path.Combine(outputPath, "let_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "mod":
                        histogramImageName = Path.Combine(outputPath, "mod_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "super":
                        histogramImageName = Path.Combine(outputPath, "super_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                    case "svar":
                        histogramImageName = Path.Combine(outputPath, "svar_avg_histogram.bmp");
                        img.Save(histogramImageName);
                        break;
                }
            }

        }

        public static Bitmap GenerateHistogram(Bitmap grayScaleImage)
        {
            //Declare new array to count 0-255
            var histogram = new int[256];

            //24bit image = 1 pixel value per 3 bytes


            //Image handling based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=dotnet-plat-ext-6.0
            //Feed in the bitmap image

            //Get width and height from bitmap. In the case of the cancer cells, the images are uniform (768 px width, 568 px height)
            int width = grayScaleImage.Width;
            int height = grayScaleImage.Height;

            //Lock dimensions of image into memory
            BitmapData grayscaleImageData = grayScaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            //Calculate size of the image 
            //int bytes = image_data.Stride * image_data.Height;
            int byteSize = Math.Abs(grayscaleImageData.Stride) * grayscaleImageData.Height;

            //Create new byte arrays of the same size
            byte[] originalImageBuffer = new byte[byteSize];

            //Copy the original image into buffer as suggested by Dr. Krawczyk...bad things happen if we modify the original image as we read from it. 
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //Release original image from memory
            grayScaleImage.UnlockBits(grayscaleImageData);


            byte intensity = 0;
            float maxIntensity = 0;

            //Loop through the byte array and count the gray levels
            for (int i = 0; i < byteSize; i ++)
            {
                //store the intensity value of the byte
                intensity = originalImageBuffer[i];

                //Add the intensity to the histogram array
                histogram[intensity]++;

                //Check if value of the current byte is higher than the recorded max
                if (histogram[intensity] > maxIntensity)
                {
                    //Set the new highest intensity value if its higher
                    maxIntensity = histogram[intensity];
                }
            }


            //Drawing function code borrowed from stackoverflow
            int histHeight = 288;
            Bitmap img = new Bitmap(256, histHeight);
            using (Graphics g = Graphics.FromImage(img))
            {
                //loop through all intensity values in the histogram array
                for (int i = 0; i < histogram.Length; i++)
                {
                    float pct = histogram[i] / maxIntensity;   //get ratio of given histogram intensity element compared to the maximum recorded intesnity
                    g.DrawLine(Pens.Black,
                        new Point(i, img.Height - 5),
                        new Point(i, img.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
            }
            return img;
        }
        #endregion
        #region Image Corruption Methods

        public static Bitmap GaussianNoise(Bitmap grayscaleImage, int Intensity)
        {   //user specified standard deviation from the slider
            int standardDeviation = Intensity;

            Random rnd = new Random();
            float probSum = 0;



            //Set width and height parameters
            int width = grayscaleImage.Width;
            int height = grayscaleImage.Height;

            //Create bitmap data from image
            BitmapData grayscaleImageData = grayscaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int byteSize = Math.Abs(grayscaleImageData.Stride) * grayscaleImage.Height;

            //Create image buffers
            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            //Create a "noise" byte array of the same size as others
            byte[] gaussianNoise = new byte[byteSize];

            //create gaussian distribution array for the intensities
            float[] gaussianIntensity = new float[256];

            //Copy in original image information to the byte array
            Marshal.Copy(grayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

            //release image from memory
            grayscaleImage.UnlockBits(grayscaleImageData);



            //generate a gaussian distribution using possible intensity values 0-255
            for (int i = 0; i < 256; i++)
            {
                gaussianIntensity[i] = (float)((1 / (Math.Sqrt(2 * Math.PI) * standardDeviation)) * Math.Exp(-Math.Pow(i, 2) / (2 * Math.Pow(standardDeviation, 2))));
                probSum += gaussianIntensity[i];
            }

            // int intensitySize = 256;
            // Parallel.For(0, intensitySize, i =>
            //{
            //    gaussianIntensity[i] = (float)((1 / (Math.Sqrt(2 * Math.PI) * std)) * Math.Exp(-Math.Pow(i, 2) / (2 * Math.Pow(std, 2))));
            //    sum += gaussianIntensity[i];
            //});



            //Parallel.For(0, intensitySize, i =>
            //{
            //    gaussianIntensity[i] /= sum;
            //    gaussianIntensity[i] *= byteSize;
            //    Change from round to floor.Round may round up and make an intensity occupy a bigger byte than what is available
            //   Causing a index out of bounds
            //    gaussianIntensity[i] = (int)Math.Round(gaussianIntensity[i]);
            //    gaussianIntensity[i] = (int)Math.Floor(gaussianIntensity[i]);
            //});

            //apply the distrbution to the number of affected bytes
            for (int i = 0; i < 256; i++)
            {
                gaussianIntensity[i] /= probSum;
                gaussianIntensity[i] *= byteSize;
                //Change from round to floor. Round may round up and make an intensity occupy a bigger byte than what is available
                //Causing a index out of bounds
                //gaussianIntensity[i] = (int)Math.Round(gaussianIntensity[i]);
                gaussianIntensity[i] = (int)Math.Floor(gaussianIntensity[i]);
            }

            int count = 0;

            //Parallel.For(0, intensitySize, i =>
            //{
            //    Parallel.For(0, (int)gaussianIntensity[i], j =>
            //    {
            //        gaussianNoise[j + count] = (byte)(i);

            //    });
            //    count += (int)gaussianIntensity[i];
            //});
            ////store the noise byte values as frequently as they appear
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < (int)gaussianIntensity[i]; j++)
                {
                    gaussianNoise[j + count] = (byte)(i);
                }

                //Keep track of the number of bytes written from the gaussian distribution
                count += (int)gaussianIntensity[i];
            }


            //Zero out the remainder of bytes with no noise 
            for (int i = 0; i < byteSize - count; i++)
            {
                gaussianNoise[count + i] = 0;
            }

            //Parallel.For(0, byteSize - count, i =>
            //  {
            //      gaussianNoise[count + i] = 0;
            //  });

            //Randomize the noise byte array
            gaussianNoise = gaussianNoise.OrderBy(x => rnd.Next()).ToArray();


            //loop through the byte array. since this is a 24 bit image, 3 bytes = 1 pixel, so increment step is 3 to loop through the pixels
            for (int i = 0; i < byteSize; i +=3)
            {

                //Loop through each byte ("color channel") in the pixel. It is still RGB format for whatever reason even though all 3 bytes in the pixel are set to the same color in the previous processing step
                for (int j = 0; j < 3; j++)
                {
                    //add the gaussian noise to the original image value, store in the modified buffer
                    //Write the same noise value to all 3 color channels
                    modifiedImageBuffer[i + j] = (byte)Math.Max(0, Math.Min(255, (originalImageBuffer[i + j] + gaussianNoise[i])));
                }
            }

            //Generate image from the modified buffer, same dimensions as specification as original
            Bitmap gaussianCorruptedImage = new Bitmap(width, height);
            BitmapData gaussianCorruptedImageData = gaussianCorruptedImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, gaussianCorruptedImageData.Scan0, byteSize);
            gaussianCorruptedImage.UnlockBits(gaussianCorruptedImageData);
            return gaussianCorruptedImage;
        }
        public static Bitmap ImpulseNoise(Bitmap grayScaleImage, int noiseIntensity)
        {

            if (noiseIntensity > 0)
            {
                //24bit image = 1 pixel value per 3 bytes


                //Image handling based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=dotnet-plat-ext-6.0
                //Feed in the bitmap image

                //Get width and height from bitmap. In the case of the cancer cells, the images are uniform (768 px width, 568 px height)
                int width = grayScaleImage.Width;
                int height = grayScaleImage.Height;

                //Lock dimensions of image into memory
                BitmapData grayscalegrayscaleImageData = grayScaleImage.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                //Calculate size of the image 
                //int bytes = image_data.Stride * image_data.Height;
                int byteSize = Math.Abs(grayscalegrayscaleImageData.Stride) * grayscalegrayscaleImageData.Height;

                //Create new byte arrays of the same size
                byte[] originalImageBuffer = new byte[byteSize];

                //Byte array to make modifications to
                byte[] modifiedImageBuffer = new byte[byteSize];

                //Copy the original image into buffer as suggested by Dr. Krawczyk...bad things happen if we modify the original image as we read from it. 
                Marshal.Copy(grayscalegrayscaleImageData.Scan0, originalImageBuffer, 0, byteSize);

                //Release original image from memory
                grayScaleImage.UnlockBits(grayscalegrayscaleImageData);


                //Impulse Noise aka Salt and Pepper Algorithm
                Random rnd = new Random();


                //loop through the byte array. since this is a 24 bit image, 3 bytes = 1 pixel, so increment step is 3 to loop through the pixels
                for (int i = 0; i < byteSize; i += 3)
                {

                    //I had to move the chance of execution random out of this method entirely because of a weird bug where if I compared to the input noise, it would generate within the true range ALWAYS. 
                    bool corruptionEnable = ChanceExecution(noiseIntensity);
                    int BlackorWhite = rnd.Next(0, 2);

                    //Loop through each byte ("color channel") in the pixel. It is still RGB format for whatever reason even though all 3 bytes in the pixel are set to the same color in the previous processing step
                    for (int j = 0; j < 3; j++)
                    {

                        if (corruptionEnable == true && BlackorWhite == 0)
                        {
                            //Corrupt the byte with black
                            modifiedImageBuffer[i + j] = 0;
                        }
                        if (corruptionEnable == true && BlackorWhite == 1)
                        {
                            //Corrupt the byte with white
                            modifiedImageBuffer[i + j] = 255;
                        }
                        if (corruptionEnable == false)
                        {
                            //Leave color byte uncorrupted
                            modifiedImageBuffer[i + j] = originalImageBuffer[i + j];
                        }
                    }
                }

                Bitmap impulseCorruptedImage = new Bitmap(width, height);
                BitmapData impulseCorruptedImageData = impulseCorruptedImage.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);
                Marshal.Copy(modifiedImageBuffer, 0, impulseCorruptedImageData.Scan0, byteSize);
                impulseCorruptedImage.UnlockBits(impulseCorruptedImageData);
                return impulseCorruptedImage;
            }
            else
            {
                //Do nothing. Slider was set to 0 so dont add impulse noise 
                return grayScaleImage;
            }
        }

        //public static Bitmap ImpulseNoise(byte[] grayImageBuffer, int noiseIntensity, int width, int height)
        //{

        //    if (noiseIntensity > 0)
        //    {
        //        //Calculate size of the image 
        //        //int bytes = image_data.Stride * image_data.Height;
        //        int byteSize = grayImageBuffer.Length;

        //        //Byte array to make modifications to
        //        byte[] modifiedImageBuffer = new byte[byteSize];


        //        //Impulse Noise aka Salt and Pepper Algorithm
        //        Random rnd = new Random();


        //        //loop through the byte array. since this is a 24 bit image, 3 bytes = 1 pixel, so increment step is 3 to loop through the pixels
        //        for (int i = 0; i < byteSize; i += 3)
        //        {

        //            //I had to move the chance of execution random out of this method entirely because of a weird bug where if I compared to the input noise, it would generate within the true range ALWAYS. 
        //            bool corruptionEnable = ChanceExecution(noiseIntensity);
        //            int BlackorWhite = rnd.Next(0, 2);

        //            //Loop through each byte ("color channel") in the pixel. It is still RGB format for whatever reason even though all 3 bytes in the pixel are set to the same color in the previous processing step
        //            for (int j = 0; j < 3; j++)
        //            {

        //                if (corruptionEnable == true && BlackorWhite == 0)
        //                {
        //                    //Corrupt the byte with black
        //                    modifiedImageBuffer[i + j] = 0;
        //                }
        //                if (corruptionEnable == true && BlackorWhite == 1)
        //                {
        //                    //Corrupt the byte with white
        //                    modifiedImageBuffer[i + j] = 255;
        //                }
        //                if (corruptionEnable == false)
        //                {
        //                    //Leave color byte uncorrupted
        //                    modifiedImageBuffer[i + j] = grayImageBuffer[i + j];
        //                }
        //            }
        //        }

        //        Bitmap impulseCorruptedImage = new Bitmap(width, height);
        //        BitmapData impulseCorruptedImageData = impulseCorruptedImage.LockBits(
        //            new Rectangle(0, 0, width, height),
        //            ImageLockMode.WriteOnly,
        //            PixelFormat.Format24bppRgb);
        //        Marshal.Copy(modifiedImageBuffer, 0, impulseCorruptedImageData.Scan0, byteSize);
        //        impulseCorruptedImage.UnlockBits(impulseCorruptedImageData);
        //        return impulseCorruptedImage;
        //    }
        //    else
        //    {
        //        //Do nothing. Slider was set to 0 so dont add impulse noise 
        //        Bitmap grayScaleImage = new Bitmap(width, height);
        //        BitmapData grayScaleImageData = grayScaleImage.LockBits(
        //            new Rectangle(0, 0, width, height),
        //            ImageLockMode.WriteOnly,
        //            PixelFormat.Format24bppRgb);
        //        Marshal.Copy(grayImageBuffer, 0, grayScaleImageData.Scan0, grayImageBuffer.Length);
        //        grayScaleImage.UnlockBits(grayScaleImageData);
        //        return grayScaleImage;
        //    }
        //}
        #endregion

        #region Grayscale Conversion

        //Deprecated due to poor performance. Getpixel/setpixel sucks

        //public static Bitmap RGB2GrayscaleImage(Bitmap imageBMP)
        //{
        //    //Iterate through vertical pixels
        //    for (int i = 0; i < imageBMP.Width; i++)
        //        //iterate through horizontal pixels
        //        for (int j = 0; j < imageBMP.Height; j++)
        //        {
        //            //Get the color of the pixel at coordinate
        //            Color RGB = imageBMP.GetPixel(i, j);

        //            //Get Red channel intensity
        //            int red = RGB.R;
        //            //Get green channel intensity
        //            int green = RGB.G;

        //            //Get blue channel intensity
        //            int blue = RGB.B;

        //            //Multiply each channel intensity by coefficient to turn it gray
        //            int gray = (byte)(.299 * red + 0.587 * green + 0.114 * blue);

        //            // set RGB channels to gray values
        //            red = gray;
        //            green = gray;
        //            blue = gray;

        //            //Apply color channel intensity changes to the pixel
        //            imageBMP.SetPixel(i, j, Color.FromArgb(red, green, blue));

        //        }
        //    return imageBMP;
        //}

        public static Bitmap RGB2GrayscaleImage(Bitmap originalBMP, string colorChoice)
        {
            //Much faster method

            //Accepted weighted averages for converting RGB to grayscale
            //Disabled for now
            //double redcoeff = 0.299;
            //double greencoeff = 0.587;
            //double bluecoeff = 0.114;

            int width = originalBMP.Width;
            int height = originalBMP.Height;

            //source images are 24 bit RGB images, so we have 3 bytes per pixel
            var originalgrayscaleImageData = originalBMP.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            var byteSize = Math.Abs(originalgrayscaleImageData.Stride) * originalgrayscaleImageData.Height;


            byte[] originalImageBuffer = new byte[byteSize];
            byte[] modifiedImageBuffer = new byte[byteSize];

            Marshal.Copy(originalgrayscaleImageData.Scan0, originalImageBuffer, 0, originalImageBuffer.Length);

            //Loop through the byte array. step size is 3 due to 3 bytes per pixel (only valid for 24 bit images)
            for (var i = 0; i < byteSize; i += 3)
            {
                byte grayVal = 0;

                //Determine gray value of the "new" byte. there are two accepted ways of doing this. For "human vision" we apply weighted coefficients to each byte of a pixel. For "computer vision" we add RGB and divide by 3. 
                // 24 bits per pixel divided by 8 = 3 bytes per pixel. We are going to 8 bit image , so the value of 3 bytes will be the same. 
                //I'm not sure about the sequence of the colors. The image is RGB but Bitmaps are encoded in BGR so.....

                //Coefficient method for taking RGB to grayscale
                //var grayVal = (byte)((0.114 * modifiedImageBuffer[i] + 0.587 * modifiedImageBuffer[i + 1] + 0.299 * modifiedImageBuffer[i + 2]));

                //Add up the 3 bytes then divide by 3, in essence taking an average of the 3 bytes and applying it across all 3 bytes. 
                switch (colorChoice)
                {
                    //Set green and blue channels to zero, preserve red
                    case "B":
                        grayVal = (byte)(originalImageBuffer[i] + originalImageBuffer[i + 1] * 0 + originalImageBuffer[i + 2] * 0);
                        modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
                        break;

                    //Set red and blue channels to zero, preserve green
                    case "G":
                        grayVal = (byte)(originalImageBuffer[i] * 0 + originalImageBuffer[i + 1] + originalImageBuffer[i + 2] * 0);
                        modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
                        break;

                    //Set green and red channels to zero, preserve 
                    case "R":
                        grayVal = (byte)(originalImageBuffer[i] * 0 + originalImageBuffer[i + 1] * 0 + originalImageBuffer[i + 2]);
                        modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
                        break;

                    case "RGB":
                        grayVal = (byte)((originalImageBuffer[i] + originalImageBuffer[i + 1] + originalImageBuffer[i + 2]) / 3);
                        modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
                        break;

                    default:
                        grayVal = (byte)((originalImageBuffer[i] + originalImageBuffer[i + 1] + originalImageBuffer[i + 2]) / 3);
                        modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
                        break;
                }
            }

            Marshal.Copy(modifiedImageBuffer, 0, originalgrayscaleImageData.Scan0, modifiedImageBuffer.Length);
            originalBMP.UnlockBits(originalgrayscaleImageData);


            Bitmap grayscaleImage = new Bitmap(width, height);
            BitmapData grayscaleImageData = grayscaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);        //I have no idea why I cannot set this to 8bit image format....... 
            Marshal.Copy(modifiedImageBuffer, 0, grayscaleImageData.Scan0, modifiedImageBuffer.Length);
            grayscaleImage.UnlockBits(grayscaleImageData);

            //var grayImage = Path.Combine(Settings.Default.OutputPath, "grayscale.bmp");
            //grayscaleImage.Save(grayImage);
            return grayscaleImage;
        }

        //public static byte[] RGB2GrayscaleImage(Bitmap originalBMP, string colorChoice)
        //{
        //    //Much faster method

        //    //Accepted weighted averages for converting RGB to grayscale
        //    //Disabled for now
        //    //double redcoeff = 0.299;
        //    //double greencoeff = 0.587;
        //    //double bluecoeff = 0.114;

        //    int width = originalBMP.Width;
        //    int height = originalBMP.Height;

        //    //source images are 24 bit RGB images, so we have 3 bytes per pixel
        //    var originalgrayscaleImageData = originalBMP.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
        //        PixelFormat.Format24bppRgb);

        //    var byteSize = Math.Abs(originalgrayscaleImageData.Stride) * originalgrayscaleImageData.Height;


        //    byte[] originalImageBuffer = new byte[byteSize];
        //    byte[] modifiedImageBuffer = new byte[byteSize];

        //    Marshal.Copy(originalgrayscaleImageData.Scan0, originalImageBuffer, 0, originalImageBuffer.Length);

        //    //Loop through the byte array. step size is 3 due to 3 bytes per pixel (only valid for 24 bit images)
        //    for (var i = 0; i < byteSize; i += 3)
        //    {
        //        byte grayVal = 0;

        //        //Determine gray value of the "new" byte. there are two accepted ways of doing this. For "human vision" we apply weighted coefficients to each byte of a pixel. For "computer vision" we add RGB and divide by 3. 
        //        // 24 bits per pixel divided by 8 = 3 bytes per pixel. We are going to 8 bit image , so the value of 3 bytes will be the same. 
        //        //I'm not sure about the sequence of the colors. The image is RGB but Bitmaps are encoded in BGR so.....

        //        //Coefficient method for taking RGB to grayscale
        //        //var grayVal = (byte)((0.114 * modifiedImageBuffer[i] + 0.587 * modifiedImageBuffer[i + 1] + 0.299 * modifiedImageBuffer[i + 2]));

        //        //Add up the 3 bytes then divide by 3, in essence taking an average of the 3 bytes and applying it across all 3 bytes. 
        //        switch (colorChoice)
        //        {
        //            //Set green and blue channels to zero, preserve red
        //            case "B":
        //                grayVal = (byte)(originalImageBuffer[i] + originalImageBuffer[i + 1] * 0 + originalImageBuffer[i + 2] * 0);
        //                modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
        //                break;

        //            //Set red and blue channels to zero, preserve green
        //            case "G":
        //                grayVal = (byte)(originalImageBuffer[i] * 0 + originalImageBuffer[i + 1] + originalImageBuffer[i + 2] * 0);
        //                modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
        //                break;

        //            //Set green and red channels to zero, preserve 
        //            case "R":
        //                grayVal = (byte)(originalImageBuffer[i] * 0 + originalImageBuffer[i + 1] * 0 + originalImageBuffer[i + 2]);
        //                modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
        //                break;

        //            case "RGB":
        //                grayVal = (byte)((originalImageBuffer[i] + originalImageBuffer[i + 1] + originalImageBuffer[i + 2]) / 3);
        //                modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
        //                break;

        //            default:
        //                grayVal = (byte)((originalImageBuffer[i] + originalImageBuffer[i + 1] + originalImageBuffer[i + 2]) / 3);
        //                modifiedImageBuffer[i] = modifiedImageBuffer[i + 1] = modifiedImageBuffer[i + 2] = grayVal;
        //                break;
        //        }
        //    }

        //    Marshal.Copy(modifiedImageBuffer, 0, originalgrayscaleImageData.Scan0, modifiedImageBuffer.Length);
        //    originalBMP.UnlockBits(originalgrayscaleImageData);


        //    Bitmap grayscaleImage = new Bitmap(width, height);
        //    BitmapData grayscaleImageData = grayscaleImage.LockBits(
        //        new Rectangle(0, 0, width, height),
        //        ImageLockMode.WriteOnly,
        //        PixelFormat.Format24bppRgb);        //I have no idea why I cannot set this to 8bit image format....... 
        //    Marshal.Copy(modifiedImageBuffer, 0, grayscaleImageData.Scan0, modifiedImageBuffer.Length);
        //    grayscaleImage.UnlockBits(grayscaleImageData);

        //    var grayImage = Path.Combine(Settings.Default.OutputPath, "grayscale.bmp");
        //    grayscaleImage.Save(grayImage);
        //    return modifiedImageBuffer;
        //}

        #endregion
        #endregion
        #region Helper Methods

        public static bool ChanceExecution(int noiseintensity)
        {
            bool execute = false;

            Random random = new Random();

            int randomint = random.Next(0, 501);
            if (randomint <= noiseintensity)
            {
                execute = true;
            }
            return execute;
        }

        public static int[] AddArrays(int[] a, int[] b)
        {
            int[] newArray = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                newArray[i] = a[i] + b[i];
            }
            return newArray;
        }

        //Recycled code from stackoverflow just to get the preview window working
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap CreateImage(byte[] grayscaleImageBuffer, int width, int height)
        {
            Bitmap grayscaleImage = new Bitmap(width, height);
            BitmapData grayscaleImageData = grayscaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);        //I have no idea why I cannot set this to 8bit image format....... 
            Marshal.Copy(grayscaleImageBuffer, 0, grayscaleImageData.Scan0, grayscaleImageBuffer.Length);
            grayscaleImage.UnlockBits(grayscaleImageData);
            return grayscaleImage;
        }

        #endregion
    }
}
