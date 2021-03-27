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
            public int actionFrame;
            public UnityEngine.Events.UnityAction<float> callback;

            public class SearchInfo
            {
                public Rect position;
                public string callerFilePath;
                public int callerLineNumber;
            }
            public SearchInfo searchInfo;

            public Element(DUIType uiType, GameObject gameObject, Rect position, string callerFilePath, int callerLineNumber)
            {
                this.uiType = uiType;
                this.gameObject = gameObject;
                this.actionFrame = 0;
                this.callback = null;
                this.searchInfo = new SearchInfo()
                {
                    position = position,
                    callerFilePath = callerFilePath,
                    callerLineNumber = callerLineNumber,
                };
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

        static Element search(DUIType uiType, Rect position, string callerFilePath, int callerLineNumber, GameObject prefab)
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
                if (uiType == elem.uiType &&
                    callerFilePath == elem.searchInfo.callerFilePath &&
                    callerLineNumber == elem.searchInfo.callerLineNumber)
                {
                    float cost = (position.position - elem.searchInfo.position.position).magnitude;
                    cost += (position.size - elem.searchInfo.position.size).magnitude;
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
                elementDict_[bestKey].searchInfo.position = position;
                return elementDict_[bestKey];
            }

            // Not found -> Create a new element.
            if (false == prefabDict_.ContainsKey(uiType))
            {
                ResetDefaultPrefab(uiType);
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
            elementDict_[newKey] = new Element(uiType, gameObject, position, callerFilePath, callerLineNumber);
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
                    var scrollRect = gameObject.GetComponent<UnityEngine.UI.ScrollRect>();
                    element.callback = (_) => element.actionFrame = Time.frameCount;
                    scrollRect.verticalScrollbar.onValueChanged.AddListener(element.callback);
                    scrollRect.horizontalScrollbar.onValueChanged.AddListener(element.callback);
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

        public static void SetDefaultPrefab(DUIType uiType, GameObject prefab)
        {
            prefabDict_[uiType] = prefab;
        }

        public static void ResetDefaultPrefab(DUIType uiType)
        {
            if (prefabDict_.ContainsKey(uiType))
            {
                GameObject.Destroy(prefabDict_[uiType]);
            }
            string prefabPath = DUISettings.PrefabPathDict[uiType];
            prefabDict_[uiType] = Resources.Load<GameObject>(prefabPath);
        }

        //------------------------------------------------------------------------------

        public static void Box(Rect position, string text,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.Box, position, callerFilePath, callerLineNumber, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static bool Button(Rect position, string text,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.Button, position, callerFilePath, callerLineNumber, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            bool clicked = elem.actionFrame == Time.frameCount;

            return clicked;
        }

        public static string TextField(Rect position, string text,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.TextField, position, callerFilePath, callerLineNumber, prefab);
            move(elem.gameObject, position);
            // TODO
            var inputField = elem.gameObject.GetComponent<UnityEngine.UI.InputField>();
            if (elem.actionFrame != Time.frameCount)
            {
                inputField.text = text;
            }
            return elem.gameObject.GetComponent<UnityEngine.UI.InputField>().text;
        }


        public static void Label(Rect position, string text,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.Label, position, callerFilePath, callerLineNumber, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);
        }

        public static float HorizontalSlider(Rect position, float value, float minValue, float maxValue,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.HorizontalSlider, position, callerFilePath, callerLineNumber, prefab);
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

        public static bool Toggle(Rect position, bool value, string text,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();
            var elem = search(DUIType.Toggle, position, callerFilePath, callerLineNumber, prefab);
            move(elem.gameObject, position);
            setText(elem.gameObject, text);

            if (elem.actionFrame != Time.frameCount)
            {
                elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn = value;
            }

            return elem.gameObject.GetComponent<UnityEngine.UI.Toggle>().isOn;
        }

        static List<GameObject> uiStack_ = null;

        static bool firstT;

        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect,
            GameObject prefab = null,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            setup();

            var elem = search(DUIType.ScrollView, position, callerFilePath, callerLineNumber, prefab);
            var viewport = elem.gameObject.transform.Find("Viewport");
            var content = viewport.Find("Content");

            var scrollRect = elem.gameObject.GetComponent<UnityEngine.UI.ScrollRect>();
            scrollRect.horizontalScrollbar.onValueChanged.RemoveListener(elem.callback);
            scrollRect.verticalScrollbar.onValueChanged.RemoveListener(elem.callback);

            move(elem.gameObject, position);

            var rect = content.gameObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = viewRect.size;

            if (elem.actionFrame != Time.frameCount)
            {
                scrollRect.horizontalScrollbar.value = scrollPosition.x / (viewRect.width - position.width);
                scrollRect.verticalScrollbar.value = 1 - scrollPosition.y / (viewRect.height - position.height);
            }
            scrollRect.horizontalScrollbar.onValueChanged.AddListener(elem.callback);
            scrollRect.verticalScrollbar.onValueChanged.AddListener(elem.callback);

            uiStack_.Add(content.gameObject);
            var newPos = new Vector2(
                (viewRect.width - position.width) * scrollRect.horizontalScrollbar.value,
                (viewRect.height - position.height) * (1 - scrollRect.verticalScrollbar.value));

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