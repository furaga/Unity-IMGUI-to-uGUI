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

    static GameObject dui_ = null;
    static Dictionary<uint, Element> elementDict_ = new Dictionary<uint, Element>();
    static Dictionary<UIType, GameObject> prefabDict_ = new Dictionary<UIType, GameObject>();
    static HashSet<uint> alreadySelected_ = new HashSet<uint>();
    static uint counter_ = 0;
    static int lastFrame_ = -1;

    static uint nextID()
    {
        counter_ += 1;
        return counter_;
    }

    static GameObject search(UIType uiType, Rect position)
    {
        if (lastFrame_ != Time.frameCount)
        {
            lastFrame_ = Time.frameCount;
            alreadySelected_.Clear();
        }

        float bestCost = float.MaxValue;
        uint bestKey = 0;

        foreach (uint key in elementDict_.Keys)
        {
            if (alreadySelected_.Contains(key))
            {
                continue;
            }
            var elem = elementDict_[key];
            if (uiType == elem.uiType)
            {
                float cost = (position.position - elem.position.position).magnitude;
                cost += (position.size - elem.position.size).magnitude;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestKey = key;
                }
                if (bestCost <= 0)
                {
                    break;
                }
            }
        }

        if (bestKey > 0)
        {
            alreadySelected_.Add(bestKey);
            return elementDict_[bestKey].gameObject;
        }

        Debug.Log("Not Found " + bestCost);
        // Not found
        if (false == prefabDict_.ContainsKey(uiType))
        {
            prefabDict_[uiType] = Resources.Load<GameObject>("Prefab/Button");
        }
        var canvas = GameObject.FindObjectOfType<Canvas>();
        var gameObject = GameObject.Instantiate(prefabDict_[uiType], dui_.transform);
        uint newKey = nextID();
        elementDict_[newKey] = new Element(uiType, gameObject, position);
        alreadySelected_.Add(newKey);
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
        if (dui_ == null)
        {
            var prefab = Resources.Load<GameObject>("Prefab/DUI");
            var canvas = GameObject.FindObjectOfType<Canvas>();
            dui_ = GameObject.Instantiate(prefab, canvas.gameObject.transform);
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
