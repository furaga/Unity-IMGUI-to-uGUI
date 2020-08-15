using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGUI;

public class Demo : MonoBehaviour
{
    void Update()
    {
        onDUI();
    }

    int pressCount_ = 0;

    void onDUI()
    {
        float offset = 0;
        DUI.Box(new Rect(30 + offset, 30, 200, 30), "DBox");
        if (DUI.Button(new Rect(30 + offset, 70, 200, 30), string.Format("DButton (Pressed {0} Times)", pressCount_)))
        {
            pressCount_ += 1;
        }
        DUI.TextField(new Rect(30 + offset, 110, 200, 30), "DTextField");
        DUI.Label(new Rect(30 + offset, 150, 200, 30), "DLabel");
        DUI.HorizontalSlider(new Rect(30 + offset, 190, 200, 30), 0.3f, 0, 1);
        DUI.Toggle(new Rect(30 + offset, 230, 200, 30), true, "DToggle");
    }

    void OnGUI()
    {
        float offset = 240;
        GUI.Box(new Rect(30 + offset, 30, 200, 30), "DBox");
        if (GUI.Button(new Rect(30 + offset, 70, 200, 30), string.Format("DButton (Pressed {0} Times)", pressCount_)))
        {
            pressCount_ += 1;
        }
        GUI.TextField(new Rect(30 + offset, 110, 200, 30), "DTextField");
        GUI.Label(new Rect(30 + offset, 150, 200, 30), "DLabel");
        GUI.HorizontalSlider(new Rect(30 + offset, 190, 200, 30), 0.3f, 0, 1);
        GUI.Toggle(new Rect(30 + offset, 230, 200, 30), true, "DToggle");
    }
}