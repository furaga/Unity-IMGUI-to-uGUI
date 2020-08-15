using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DUI 
{
    enum UIType
    {
        Button,
    }

    class Element
    {
        public UIType uiType;
        public GameObject gameObject;
        public Rect position;
        public Element(UIType uiType, GameObject gameObject, Rect position)
        {
            this.uiType = uiType;
            this.gameObject = gameObject;
            this.position = position;
        }
    }

    static Dictionary<uint, Element> elementDict = new Dictionary<uint, Element>();
    static Dictionary<UIType, GameObject> prefabDict = new Dictionary<UIType, GameObject>();
    static uint counter_ = 0;

    static void setup()
    {
    }

    static GameObject search(UIType uiType, Rect position)
    {
        foreach (uint key in elementDict.Keys)
        {
            var elem = elementDict[key];
            if (uiType == elem.uiType && position == elem.position)
            {
                return elem.gameObject;
            }
        }

        if (false == prefabDict.ContainsKey(uiType))
        {
            prefabDict[uiType] = Resources.Load<GameObject>("Prefab/Button");
        }

        var canvas = GameObject.FindObjectOfType<Canvas>();
        var gameObject = GameObject.Instantiate(prefabDict[uiType], canvas.gameObject.transform);
        elementDict[counter_] = new Element(uiType, gameObject, position);
        return gameObject;
    }

    public static bool Button(Rect position, string content)
    {
        setup();
        var gameObject = search(UIType.Button, position);
        var buttonText = gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        buttonText.text = content;
        return false;
    }

}
