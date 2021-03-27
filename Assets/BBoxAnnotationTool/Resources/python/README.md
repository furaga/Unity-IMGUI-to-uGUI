# Visualizers

## Requirements

- Python>=3.6
  - We confirmed it works with an Anaconda virtual environment, `conda create -n pat pip=9.0 python=3.6`

## Setup

```
$ pip install -r requirements.txt
```

## Visualize annotation

```
$ python visualize_bbox.py --image_dir <image directory which contains keypoints.json>
```

#### Example command

```
$ python visualize_bbox.py --image_dir ../Annotations
```