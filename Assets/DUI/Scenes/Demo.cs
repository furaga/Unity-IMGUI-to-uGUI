using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    void Update()
    {
        onDUI();
    }

    int pressCount_ = 0;

    void onDUI()
    {
        if (DUI.Button(new Rect(100, 100, 200, 30), string.Format("Button (Pressed {0} Times)", pressCount_)))
        {
            pressCount_ += 1;
        }
    }
}
