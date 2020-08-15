using System.Collections;
using System.Collections.Generic;

namespace EasyGUI
{
    public enum DUIType
    {
        None,
        Button,
        Label,
        InputField,
        Box,
    }

    public static class DUISettings
    {
        public static Dictionary<DUIType, string> PrefabPathDict = new Dictionary<DUIType, string>()
        {
            { DUIType.Button, "Prefab/Button" },
            { DUIType.Label, "Prefab/Label" },
            { DUIType.InputField, "Prefab/InputField" },
            { DUIType.Box, "Prefab/Box" },
        };
    }
}