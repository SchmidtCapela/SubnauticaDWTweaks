using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SeamothStorageContainer))]
    [HarmonyPatch("Init")]
    class SeamothStorageContainer_Init_patch
    {
        public static readonly object fieldWidth = AccessTools.Field(typeof(SeamothStorageContainer), "width");
        public static readonly object fieldHeight = AccessTools.Field(typeof(SeamothStorageContainer), "height");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if ((DW_Tweaks_Settings.Instance.SeamothInventoryWidth == 4) && (DW_Tweaks_Settings.Instance.SeamothInventoryHeight == 4)) return false;
            else return true;
        }

        public static void Prefix(SeamothStorageContainer __instance)
        {
            __instance.width = DW_Tweaks_Settings.Instance.SeamothInventoryWidth;
            __instance.height = DW_Tweaks_Settings.Instance.SeamothInventoryHeight;
            return;
        }
    }
}
