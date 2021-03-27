using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGUI;

public class Demo : MonoBehaviour
{
    public GameObject Box;
    public GameObject Button;
    public GameObject TextField;
    public GameObject Label;
    public GameObject ScrollView;
    public GameObject HorizontalSlider;
    public GameObject Toggle;

    void Start()
    {
        if (Box != null) DUI.SetDefaultPrefab(DUIType.Box, Box);
        if (Button != null) DUI.SetDefaultPrefab(DUIType.Button, Button);
        if (TextField != null) DUI.SetDefaultPrefab(DUIType.TextField, TextField);
        if (Label != null) DUI.SetDefaultPrefab(DUIType.Label, Label);
        if (ScrollView != null) DUI.SetDefaultPrefab(DUIType.ScrollView, ScrollView);
        if (HorizontalSlider != null) DUI.SetDefaultPrefab(DUIType.HorizontalSlider, HorizontalSlider);
        if (Toggle != null) DUI.SetDefaultPrefab(DUIType.Toggle, Toggle);
    }

    void Update()
    {
        onDUI();
    }

    int button_ = 0;
    string textField_ = "DTextField";
    float slider_ = 0.3f;
    bool toggle_ = true;
    Vector2 scroll_;

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

        if (Time.time % 2 < 1)
        {
            DUI.Label(new Rect(30 + ox, 400, 200, 30), "Elapsed " + Time.time);
        }

        // This line is necessary to hide unused GUI elements.
        DUI.Flush();
    }

    void OnGUI()
    {
        float ox = 240;
        GUI.Box(new Rect(30 + ox, 30, 200, 30), "GBox");
        if (GUI.Button(new Rect(30 + ox, 70, 200, 30), string.Format("GButton (Pressed {0} Times)", button_)))
        {
            button_ += 1;
        }
        textField_ = GUI.TextField(new Rect(30 + ox, 110, 200, 30), textField_);
        GUI.Label(new Rect(30 + ox, 150, 200, 30), "GLabel");
        slider_ = GUI.HorizontalSlider(new Rect(30 + ox, 190, 200, 30), slider_, 0, 1);
        toggle_ = GUI.Toggle(new Rect(30 + ox, 230, 200, 30), toggle_, "GToggle");

        scroll_ = GUI.BeginScrollView(new Rect(30 + ox, 270, 200, 80), scroll_, new Rect(0, 0, 400, 120));
        {
            GUI.Label(new Rect(10, 10, 180, 30), "GLabel in Scroll View");
        }
        GUI.EndScrollView();

        if (Time.time % 2 < 1)
        {
            GUI.Label(new Rect(30 + ox, 400, 200, 30), "Elapsed " + Time.time);
        }
    }
}