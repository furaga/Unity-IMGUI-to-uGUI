using System.Collections;
using System.Collections.Generic;

namespace EasyGUI
{
    public enum DUIType
    {
        None,
        Box,
        Button,
        TextField,
        Label,
        ScrollView,
        HorizontalSlider,
        Toggle,
        Dropdown,
    }

    public static class DUISettings
    {
        public static Dictionary<DUIType, string> PrefabPathDict = new Dictionary<DUIType, string>()
        {
            { DUIType.Box, "Prefab/Box" },
            { DUIType.Button, "Prefab/Button" },
            { DUIType.TextField, "Prefab/InputField" },
            { DUIType.Label, "Prefab/Label" },
            { DUIType.ScrollView, "Prefab/Scroll View" },
            { DUIType.HorizontalSlider, "Prefab/Slider" },
            { DUIType.Toggle, "Prefab/Toggle" },
            { DUIType.Dropdown, "Prefab/Dropdown" },
        };
    }
}