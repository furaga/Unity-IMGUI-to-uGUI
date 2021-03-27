"""
$ python visualize_bbox.py --image_dir ../Annotations
"""

import argparse
from collections import namedtuple
import cv2
import os
import numpy as np
import random
import colorsys
import pandas as pd


def parse_args():
    parser = argparse.ArgumentParser(description='Visualize Annotations')
    parser.add_argument('--image_dir', type=str, default="../Annotations")
    args = parser.parse_args()
    return args


def get_colormap(labels):
    labels = set(labels)
    colormap = {}
    for label in labels:
        h, s, l = random.random(), 0.5 + random.random()/2.0, 0.4 + random.random()/5.0
        r, g, b = [int(256*i) for i in colorsys.hls_to_rgb(h, l, s)]
        colormap[label] = (b, g, r)
    return colormap


def main(args):
    annot_path = os.path.join(args.image_dir, "bboxes.csv")
    if not os.path.exists(annot_path):
        print("Not found:", annot_path)
        return
    df = pd.read_csv(annot_path, encoding="utf8")
    colormap = get_colormap(df.values[:, 1])

    ann_dict = {}
    for img_name, label, x, y, w, h in df.values:
        bbox = (label, x, y, w, h)
        if img_name not in ann_dict:
            ann_dict[img_name] = []
        ann_dict[img_name].append(bbox)

    for img_name, bboxes in ann_dict.items():
        img = cv2.imread(os.path.join(args.image_dir, img_name))
        ih, iw = img.shape[:2]

        rendered = img.copy()
        for label, x, y, w, h in bboxes:
            x = int(x * iw)
            y = int(y * ih)
            w = int(w * iw)
            h = int(h * ih)
            cv2.rectangle(rendered, (x, y), (x+w, y+h), colormap[label])
            cv2.putText(rendered, label, (x + 5, y + 20),
                        cv2.FONT_HERSHEY_DUPLEX, 0.5, (255, 255, 255), 1, cv2.LINE_AA)

        # show rendered image
        show_img = cv2.hconcat([img, rendered])
        cv2.imshow(annot_path, show_img)
        if ord('q') == cv2.waitKey(0):
            break


if __name__ == '__main__':
    args = parse_args()
    main(args)
