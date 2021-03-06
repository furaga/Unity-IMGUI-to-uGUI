# UnityDUI

![code](doc/code.png)

![screenshot](doc/screenshot.png)

## What is This

Unity has two UI systems, IMGUI and uGUI.  
IMGUI is code-based UI system (ex. GUI.Button()).  
uGUI is GameObject-based UI system. (ex. "Unity Editor's main menu > GameObject > UI > Button")

[An official document](https://docs.unity3d.com/2019.4/Documentation/Manual/GUIScriptingGuide.html) says 
that we should not use IMGUI for normal in-game UI, and should use uGUI instead.  

However, for programmers, IMGUI is very convienient because we can create and control UI elements in script.  
On the other hand, uGUI is very time-consuming and stressfull because we need create and locate UI elements manually.  

Therefore, we implemented **a code-based interface to create and control uGUI elements like IMGUI.**

For example, when you write the following code,

```
void Update()
{
    DUI.Button(new Rect(30, 30, 200, 50), "Hello DUI");
}
```

a button object will be created automatically and the line will reuse the object since the next frame.

![Button](doc/Button.png)

## How to Use

See **Assets/DUI/Scenes/Demo.cs**

```csharp

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
```
