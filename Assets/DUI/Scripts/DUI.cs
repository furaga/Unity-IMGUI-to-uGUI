using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyGUI
{
    public static class DUI
    {
        class Element
        {
            public DUIType uiType;
            public GameObject gameObject;
            public Rect position;
            public int actionFrame;
            public Element(DUIType uiType, GameObject gameObject, Rect position)
            {
                this.uiType = uiType;
                this.gameObject = gameObject;
                this.position = position;
                this.actionFrame = 0;
            }
        }

        static GameObject dui_ = null;
        static Dictionary<uint, Element> elementDict_ = new Dictionary<uint, Element>();
        static Dictionary<DUIType, GameObject> prefabDict_ = new Dictionary<DUIType, GameObject>();
        static HashSet<uint> alreadySelected_ = new HashSet<uint>();
        static uint counter_ = 0;
        static int lastFrame_ = -1;

        static uint nextID()
        {
            counter_ += 1;
            return counter_;
        }

        //------------------------------------------------------------------------------------------

        static Element search(DUIType uiType, Rect position)
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
                return elementDict_[bestKey];
            }

            Debug.Log("Not Found " + bestCost);
            // Not found
            if (false == prefabDict_.ContainsKey(uiType))
            {
                string prefabPath = DUISettings.PrefabPathDict[uiType];
                prefabDict_[uiType] = Resources.Load<GameObject>(prefabPath);
            }

            var canvas = GameObject.FindObjectOfType<Canvas>();
            var gameObject = GameObject.Instantiate(prefabDict_[uiType], dui_.transform);

            uint newKey = nextID();
            elementDict_[newKey] = new Element(uiType, gameObject, position);
            alreadySelected_.Add(newKey);

            // Add event listener
            setupEventHandler(uiType, gameObject, elementDict_[newKey]);

            return elementDict_[newKey];
        }

        static void setupEventHandler(DUIType uiType, GameObject gameObject, Element element)
        {
            switch (uiType)
            {
                case DUIType.Button:
                    gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(
                        () => element.actionFrame = Time.frameCount);
                    break;
                case DUIType.TextField:
                    gameObject.GetComponent<UnityEngine.UI.InputField>().onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
                    break;
                case DUIType.HorizontalSlider:
                    gameObject.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
                    break;
                case DUIType.Toggle:
                    gameObject.GetComponent<UnityEngine.UI.Toggle>().onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
                    break;
                case DUIType.ScrollView:
                    //gameObject.GetComponent<UnityEngine.UI.Scrollbar>().onValueChanged.AddListener(
                    //    (_) => element.actionFrame = Time.frameCount);
                    break;
            }
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

        static void setText(GameObject ui, string text)
        {
            var textComp = ui.GetComponentInChildren<UnityEngine.UI.Text>();
            textComp.text = text;
        }
        
        //------------------------------------------------------------------------------

        public static void Box(Rect position, string text)
        {
            setup();
            var elem = search(DUIType.Box, position);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static bool Button(Rect position, string text)
        {
            setup();
            var elem = search(DUIType.Button, position);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            bool clicked = elem.actionFrame == Time.frameCount;

            return clicked;
        }

        public static string TextField(Rect position, string text)
        {
            setup();
            var elem = search(DUIType.TextField, position);
            move(elem.gameObject, position);
            // TODO
            var inputField = elem.gameObject.GetComponent<UnityEngine.UI.InputField>();
            if (elem.actionFrame != Time.frameCount)
            {
                inputField.text = text;
            }
            return elem.gameObject.GetComponent<UnityEngine.UI.InputField>().text;
        }

        public static void Label(Rect position, string text)
        {
            setup();
            var elem = search(DUIType.Label, position);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static float HorizontalSlider(Rect position, float value, float minValue, float maxValue)
        {
            setup();
            var elem = search(DUIType.HorizontalSlider, position);
            move(elem.gameObject, position);

            var slider = elem.gameObject.GetComponent<UnityEngine.UI.Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            if (elem.actionFrame != Time.frameCount)
            {
                slider.value = value;
            }

            return slider.value;
        }

        public static bool Toggle(Rect position, bool value, string text)
        {
            setup();
            var elem = search(DUIType.Toggle, position);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            if (elem.actionFrame != Time.frameCount)
            {
                elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn = value;
            }

            return elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn;
        }

    }
}