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

    int button_ = 0;
    string textField_ = "DTextField";
    float slider_ = 0.3f;
    bool toggle_ = true;

    void onDUI()
    {
        float ox = 0;
        DUI.Box(new Rect(30 + ox, 30, 200, 30), "DBox");
        if (DUI.Button(new Rect(30 + ox, 70, 200, 30), string.Format("DButton (Pressed {0} Times)", button_)))
        {
            button_ += 1;
        }
        textField_ = DUI.TextField(new Rect(30 + ox, 110, 200, 30), textField_);
        DUI.Label(new Rect(30 + ox, 150, 200, 30), "DLabel");
        slider_ = DUI.HorizontalSlider(new Rect(30 + ox, 190, 200, 30), slider_, 0, 1);
        toggle_ = DUI.Toggle(new Rect(30 + ox, 230, 200, 30), toggle_, "DToggle");
    }

    void OnGUI()
    {
        float ox = 240;
        GUI.Box(new Rect(30 + ox, 30, 200, 30), "DBox");
        if (GUI.Button(new Rect(30 + ox, 70, 200, 30), string.Format("DButton (Pressed {0} Times)", button_)))
        {
            button_ += 1;
        }
        textField_ = GUI.TextField(new Rect(30 + ox, 110, 200, 30), textField_);
        GUI.Label(new Rect(30 + ox, 150, 200, 30), "DLabel");
        slider_ = GUI.HorizontalSlider(new Rect(30 + ox, 190, 200, 30), slider_, 0, 1);
        toggle_ = GUI.Toggle(new Rect(30 + ox, 230, 200, 30), toggle_, "DToggle");
    }
}