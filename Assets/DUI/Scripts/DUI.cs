using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // TODO:

        public static Matrix4x4 matrix
        {
            get
            {
                return GUI.matrix;
            }
            set
            {
                GUI.matrix = value;
            }
        }

        public static Color color
        {
            get
            {
                return GUI.color;
            }
            set
            {
                GUI.color = value;
            }
        }

        public static GUISkin skin
        {
            get
            {
                return GUI.skin;
            }
            set
            {
                GUI.skin = value;
            }
        }

        public static void DrawTexture(Rect position, Texture2D texture)
        {
            GUI.DrawTexture(position, texture);
        }

        //------------------------------------------------------------------------------------------

        static Element search(DUIType uiType, Rect position, GameObject prefab)
        {
            float bestCost = float.MaxValue;
            uint bestKey = 0;

            foreach (uint key in elementDict_.Keys)
            {
                if (alreadySelected_.Contains(key))
                {
                    continue;
                }

                var elem = elementDict_[key];

                // Check if the element is in the current hierarchy level
                if (uiStack_.Last().transform != elem.gameObject.transform.parent)
                {
                    continue;
                }

                // Select element whose rect is most similar to `position`
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

            // Found
            if (bestKey > 0)
            {
                alreadySelected_.Add(bestKey);
                return elementDict_[bestKey];
            }

            // Not found -> Create a new element.
            if (false == prefabDict_.ContainsKey(uiType))
            {
                string prefabPath = DUISettings.PrefabPathDict[uiType];
                prefabDict_[uiType] = Resources.Load<GameObject>(prefabPath);
            }
            GameObject gameObject = null;
            if (prefab != null)
            {
                gameObject = GameObject.Instantiate(prefab, uiStack_.Last().transform);
            }
            else
            {
                gameObject = GameObject.Instantiate(prefabDict_[uiType], uiStack_.Last().transform);
            }
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
                case DUIType.Dropdown:
                    gameObject.GetComponent<UnityEngine.UI.Dropdown>().onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
                    break;
                case DUIType.ScrollView:
                    var scrollRect = gameObject.GetComponent<UnityEngine.UI.ScrollRect>();
                    scrollRect.verticalScrollbar.onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
                    scrollRect.horizontalScrollbar.onValueChanged.AddListener(
                        (_) => element.actionFrame = Time.frameCount);
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
            if (lastFrame_ != Time.frameCount)
            {
                lastFrame_ = Time.frameCount;
                alreadySelected_.Clear();

                if (uiStack_ != null && uiStack_.Count >= 2)
                {
                    uiStack_.RemoveRange(1, uiStack_.Count - 1);
                }
            }

            if (dui_ == null)
            {
                var prefab = Resources.Load<GameObject>("Prefab/DUI");
                var canvas = GameObject.FindObjectOfType<Canvas>();
                dui_ = GameObject.Instantiate(prefab, canvas.gameObject.transform);
            }
            if (uiStack_ == null)
            {
                uiStack_ = new List<GameObject>();
                var canvas = GameObject.FindObjectOfType<Canvas>();
                uiStack_.Add(canvas.gameObject);
            }
        }

        static void setText(GameObject ui, string text)
        {
            var textComp = ui.GetComponentInChildren<UnityEngine.UI.Text>();
            textComp.text = text;
        }

        //------------------------------------------------------------------------------

        public static void Box(Rect position, string text, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.Box, position, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static bool Button(Rect position, string text, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.Button, position, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            bool clicked = elem.actionFrame == Time.frameCount;

            return clicked;
        }

        public static string TextField(Rect position, string text, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.TextField, position, prefab);
            move(elem.gameObject, position);
            // TODO
            var inputField = elem.gameObject.GetComponent<UnityEngine.UI.InputField>();
            if (elem.actionFrame != Time.frameCount)
            {
                inputField.text = text;
            }
            return elem.gameObject.GetComponent<UnityEngine.UI.InputField>().text;
        }

        public static void Label(Rect position, string text, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.Label, position, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static void Label(Rect position, string text, GUIStyle style)
        {
            GUI.Label(position, text, style);
        }

        public static float HorizontalSlider(Rect position, float value, float minValue, float maxValue, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.HorizontalSlider, position, prefab);
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

        public static bool Toggle(Rect position, bool value, string text, GameObject prefab = null)
        {
            setup();
            var elem = search(DUIType.Toggle, position, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            if (elem.actionFrame != Time.frameCount)
            {
                elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn = value;
            }

            return elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn;
        }

        //public static DropdownState Dropdown(Rect position, string[] options, GUIEx.DropdownState state, GameObject prefab = null)
        //{
        //    setup();
        //    var elem = search(DUIType.DropDown, position, prefab);
        //    move(elem.gameObject, position);

        //    // 

        //    return state;
        //}


        static List<GameObject> uiStack_ = null;


        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GameObject prefab = null)
        {
            setup();

            var elem = search(DUIType.ScrollView, position, prefab);
            var viewport = elem.gameObject.transform.Find("Viewport");
            var content = viewport.Find("Content");

            move(elem.gameObject, position);
            move(content.gameObject, viewRect);

            var scrollRect = elem.gameObject.GetComponent<UnityEngine.UI.ScrollRect>();

            scrollRect.horizontalScrollbar.onValueChanged.RemoveAllListeners();
            scrollRect.verticalScrollbar.onValueChanged.RemoveAllListeners();
            if (elem.actionFrame != Time.frameCount)
            {
                scrollRect.horizontalScrollbar.value = scrollPosition.x / (viewRect.width - position.width);
                scrollRect.verticalScrollbar.value = 1 - scrollPosition.y / (viewRect.height - position.height);
            }
            scrollRect.horizontalScrollbar.onValueChanged.AddListener((_) => elem.actionFrame = Time.frameCount);
            scrollRect.verticalScrollbar.onValueChanged.AddListener((_) => elem.actionFrame = Time.frameCount);

            uiStack_.Add(content.gameObject);
            var newPos = new Vector2(
                (viewRect.width - position.width) * scrollRect.horizontalScrollbar.value,
                (viewRect.height - position.height) * (1 - scrollRect.verticalScrollbar.value));

            Debug.Log(elem.actionFrame + ": " + newPos.x + ", " + newPos.y);
            return newPos;
        }

        public static void EndScrollView()
        {
            setup();
            if (uiStack_.Count <= 0)
            {
                return;
            }
            var last = uiStack_.Last();
            if (last.GetComponent<UnityEngine.UI.Scrollbar>())
            {
                uiStack_.RemoveAt(uiStack_.Count - 1);
            }
        }
    }
}