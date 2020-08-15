using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        onDUI();
    }

    // Update is called once per frame
    void onDUI()
    {
        float t = Time.time * Mathf.PI;
        float dx = 50 * Mathf.Cos(t);
        float dy = 50 * Mathf.Sin(t);
        DUI.Button(new Rect(100 + dx, 100 + dy, 150, 30), string.Format("Button ({0})", Time.frameCount));
        DUI.Button(new Rect(150 + dx, 200 + dy, 150, 30), string.Format("Button2 ({0})", Time.frameCount));
    }
}
