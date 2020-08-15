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

    static GameObject root_ = null;
    static Dictionary<uint, Element> elementDict = new Dictionary<uint, Element>();
    static Dictionary<UIType, GameObject> prefabDict = new Dictionary<UIType, GameObject>();
    static uint counter_ = 0;

    static uint nextID()
    {
        counter_ += 1;
        return counter_;
    }

    static GameObject search(UIType uiType, Rect position)
    {
        float bestCost = float.MaxValue;
        Element bestElem = null;

        foreach (uint key in elementDict.Keys)
        {
            var elem = elementDict[key];
            if (uiType == elem.uiType)
            {
                float cost = (position.position - elem.position.position).magnitude;
                cost += (position.size - elem.position.size).magnitude;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestElem = elem;
                }
                if (bestCost <= 0)
                {
                    break;
                }
            }
        }

        if (bestElem != null)
        {
            return bestElem.gameObject;
        }

        Debug.Log("Not Found " + bestCost);
        // Not found
        if (false == prefabDict.ContainsKey(uiType))
        {
            prefabDict[uiType] = Resources.Load<GameObject>("Prefab/Button");
        }
        var canvas = GameObject.FindObjectOfType<Canvas>();
        var gameObject = GameObject.Instantiate(prefabDict[uiType], root_.transform);
        elementDict[nextID()] = new Element(uiType, gameObject, position);
        return gameObject;
    }

    static void move(GameObject ui, Rect position)
    {
        var rect = ui.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(position.x, -position.y);
        rect.sizeDelta = position.size;
    }

    static void setup()
    {
        if (root_ == null)
        {
            var prefab = Resources.Load<GameObject>("Prefab/DUI");
            var canvas = GameObject.FindObjectOfType<Canvas>();
            root_ = GameObject.Instantiate(prefab, canvas.gameObject.transform);
        }
    }

    public static bool Button(Rect position, string content)
    {
        setup();
        var gameObject = search(UIType.Button, position);
        move(gameObject, position);
        var buttonText = gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        buttonText.text = content;
        return false;
    }

}
