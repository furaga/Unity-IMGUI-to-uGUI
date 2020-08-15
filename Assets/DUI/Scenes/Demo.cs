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
        if (DUI.Button(new Rect(30, 30, 200, 30), string.Format("Button (Pressed {0} Times)", pressCount_)))
        {
            pressCount_ += 1;
        }

        DUI.Label(new Rect(30, 70, 200, 30), "Label");
    }

    void OnGUI()
    {
        GUI.Button(new Rect(250, 30, 200, 30), string.Format("Button (Pressed {0} Times)", pressCount_));
        GUI.Label(new Rect(250, 70, 200, 30), "Label");
    }
}