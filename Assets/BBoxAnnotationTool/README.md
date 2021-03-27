# **BBoxAnnotationTool**

<img src=Resources/Docs/screenshot00.png height="300">

**BBoxAnnotationTool is a simple annotation tool to draw bounding boxes.**  
You can draw bounding boxes of objects in images, and dump them to a CSV file.   


## **Features**

- A tool for bounding boxes annotation purely written in C#.
- A python script to visualize annotations


## **How to Use**

<img src=Resources/Docs/UIs.png height="300">

1. Import BBoxAnnotationTool.unitypackage

2. Open and run **Assets/BBoxAnnotationTool/Scenes/BBoxAnnotationTool.unity**.

3. Click **..** button on Save/Load Panel and select an image directory that contains images you would like to annotate.  
(Or, write the directory path in the neighboring text field and click "Reload" button)

4. Click on Canvas to draw bounding boxes.  
You can select label by changing the value of the drop down on the top of Canvas. 

5. Press **◁** and **▷** buttons in Seek Bar to switch images. 

6. Press **Save** button to save your annotations. The annotations will be saved in **[the-image-directory]/bboxes.csv**. 
The csv format is: 
```
ImageName,Label,X,Y,W,H
<image name1>,<label1>,<x1>,<y1>,<w1>,<h1>
<image name1>,<label1>,<x2>,<y2>,<w2>,<h2>
...
```


7. Optionally, you can visualize the saved annotations with **Assets/BBoxAnnotationTool/Resources/python/visualize_bbox.py**  
We confirmed it works with Python 3.6.10. 

```
$ cd Assets/BBoxAnnotationTool/Resources/python
$ pip install -r requirements.txt
$ python visualize_bbox.py --image_dir <image directory which contains segmentation.xml>
```

## **Label Set**

You can change label set by changing the property **Hierarchy > Main Camera > Segmentation Window (Script) > Label File** in Unity Editor.  
The default value is **./Assets/BBoxAnnotationTool/Resources/Settings/labels.txt**.  

```
person
face
street sign
road
grass
car
house
window
chair
sports ball
```

If you would like to use another label set, create a new label file (a simple text file, one label in one line) and set the path to the above property. 


## **Shortcut Keys**

- **Alt+S**
  - Save annotation
  - Equvalent to press **Save** button.
- **Alt+O**
  - Show a directory open dialog, and load images and already existing segmentation.xml.
  - Equvalent to press **..** button.
- **Alt+R**
  - Reload the images and already existing segmentation.xml.
  - Equvalent to press **Reload** button.
- **C**
  - Copy a focused bounding box to clipboard.
- **V**
  - Paste the bounding box in clipboard.
- **Delete**
  - Delete a focused bounding box.
- **Enter**
  - Randomly change the color of the focused contour.
- **Z**
  - Undo 
- **Y**
  - Redo 
