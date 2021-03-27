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
    Vector2 scroll_;
    GUIEx.DropdownState dropDownState_ = new GUIEx.DropdownState();

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

        scroll_ = DUI.BeginScrollView(new Rect(30 + ox, 270, 200, 80), scroll_, new Rect(0, 0, 400, 120));
        {
            DUI.Label(new Rect(10, 10, 180, 30), "DLabel in Scroll View");
        }
        DUI.EndScrollView();
    }

    void OnGUI()
    {
        float ox = 240;
        DUI.Box(new Rect(30 + ox, 30, 200, 30), "GBox");
        if (DUI.Button(new Rect(30 + ox, 70, 200, 30), string.Format("GButton (Pressed {0} Times)", button_)))
        {
            button_ += 1;
        }
        textField_ = GUI.TextField(new Rect(30 + ox, 110, 200, 30), textField_);
        DUI.Label(new Rect(30 + ox, 150, 200, 30), "GLabel");
        slider_ = DUI.HorizontalSlider(new Rect(30 + ox, 190, 200, 30), slider_, 0, 1);
        toggle_ = DUI.Toggle(new Rect(30 + ox, 230, 200, 30), toggle_, "GToggle");

        scroll_ = GUI.BeginScrollView(new Rect(30 + ox, 270, 200, 80), scroll_, new Rect(0, 0, 400, 120));
        {
            GUI.Label(new Rect(10, 10, 180, 30), "GLabel in Scroll View");
        }
        GUI.EndScrollView();

        dropDownState_ = GUIEx.Dropdown(new Rect(30 + ox, 380, 100, 30), new[] { "A", "B", "C" }, dropDownState_);
    }
}