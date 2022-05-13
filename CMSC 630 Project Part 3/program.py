#Import libraries
import os
import toml
import click
import numpy as np
from tqdm import tqdm
from pathlib import Path
from functools import partial
from multiprocessing import Pool
from click import clear, echo, style, secho
from typing import Any, List, Dict, Callable, Optional

#Import functions from utility
from functions import (
    save_features,
    load_features,
    evaluate_classifier,
    extract_features,
    predict_label,
    deserialize_label,
    normalize_data,
    labels
)

config: Dict[str, Any] = {}


@click.group(chain=True)
@click.option(
    "config_location",
    "-c",
    "--config",
    envvar="CMSC630_CONFIG",
    type=click.Path(exists=True),
    default="config.toml",
    show_default=True,
)
@click.pass_context
def main(ctx, config_location: Optional[str]) -> None:
    """
    CMSC 630 Project Part 3
    """
    global config

    try:
        config = toml.load(config_location)
    except Exception as e:
        secho(f"[ERROR] Config file error {config_location} : {e}")
        return

    Path(config["OUTPUT"]).mkdir(parents=True, exist_ok=True)


@main.command()
@click.pass_context
def extractfeatures(ctx):
    """
    Feature extraction from the 499 images
    """

    #Get path from the config file
    base_path: Path = Path(config["INPUT"])

    #Filter on the file extension BMP
    files: List = list(base_path.glob(f"*{config['FILE_EXTENSION']}"))
    echo(
        style(f"Input path: {str(base_path)}; {len(files)} images found", fg="green")
    )

    features = []
    extract_img_features = partial(extract_features, config)
    echo(
        style(f"Begin processing with {config['THREADS']} threads", fg="green")
    )
    with Pool(config["THREADS"]) as p:
        with tqdm(total=len(files)) as pbar:
            for res in tqdm(p.imap(extract_img_features, files)):
                pbar.write(res["msg"])
                if len(res["features"]) == 5:
                    features.append(res["features"])
                pbar.update()

    output_file = Path(os.path.join(config["OUTPUT"], config["FEATURES_CSV"]))
    echo(
        style(f"Saving extracted features to {output_file}; {len(features)} rows", fg="green")
    )
    normalized_dataset = normalize_data(np.array(features))
    save_features(normalized_dataset, output_file)


@main.command()
@click.pass_context
def evaluateclassifier(ctx):
    """
    Runs the KNN classifier based on the features extracted
    """

    output_file = Path(os.path.join(config["OUTPUT"], config["FEATURES_CSV"]))

    dataset = load_features(output_file)

    if len(dataset) == 0:
        echo(
            style("[ERROR] ", fg="red")
            + "Extracted features missing or empty. Program terminating."
        )
        return

    total_avg = 0
    for k in range(1, int(config["MAX_K"]) + 1, 2):
        scores = evaluate_classifier(dataset, config["N_FOLDS"], k)
        average = sum(scores) / float(len(scores))
        echo("\n")
        echo(style("", fg="green") + f"k={k}")
        echo(f"\tScores: {['{:.3f}%'.format(score) for score in scores]}")
        echo(f"\tAverage Accuracy: {average:.3f}%")
        
        total_avg += average

    total_avg /= int(config["MAX_K"])
    echo(style("\n ", fg="green") + f"Total Average: {total_avg:.3f}%")
    echo (labels)


@main.command()
@click.argument("path", nargs=1, type=click.Path(exists=True))
@click.option("k", "-k", "--k-value", default=3, show_default=True)
@click.pass_context
def predictclass(ctx, path, k):
    """
    Use KNN to perdict the label/class of specified image
    """

    features = extract_features(config, Path(path))["features"]
    if len(features) != 5:
        return

    features = np.reshape(features, (-1, 5))

    extracted_features_csv = Path(os.path.join(config["OUTPUT"], config["FEATURES_CSV"]))
    dataset = load_features(extracted_features_csv)
    normalized_dataset = normalize_data(dataset)

    label = predict_label(normalized_dataset, features, k)

    label_name = deserialize_label(int(label))

    echo("Predicted label: " + label_name)


if __name__ == "__main__":
    main()
