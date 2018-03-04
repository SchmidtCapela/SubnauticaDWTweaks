using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("OnHandHover")]
    class StorageContainer_OnHandHover_patch
    {

        public static readonly object methodGetContainer = AccessTools.Method(typeof(StorageContainer), "get_container");
        public static readonly object methodGetCount = AccessTools.Method(typeof(ItemsContainer), "get_count");
        public static readonly object funcStringFormat2 = AccessTools.Method(typeof(String), "Format", new Type[] { typeof(string), typeof(object), typeof(object) });
        public static readonly object fieldStringEmpty = AccessTools.Field(typeof(String), "Empty");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.ContainerQuantityTooltip;
        }

        public static bool Prefix(StorageContainer __instance)
        {
            if (!__instance.enabled)
            {
                return false;
            }
            Constructable component = __instance.gameObject.GetComponent<Constructable>();
            if (!component || component.constructed)
            {
                if (__instance.IsEmpty()) HandReticle.main.SetInteractText(__instance.hoverText, "Empty");
                else
                {
                    List<TechType> itemTypes = __instance.container.GetItemTypes();
                    if (itemTypes.Count() == 1) HandReticle.main.SetInteractText(__instance.hoverText, string.Format("{0} {1}", __instance.container.count, Language.main.Get(itemTypes[0].AsString())));
                    else HandReticle.main.SetInteractText(__instance.hoverText, string.Format("{0} items", __instance.container.count));
                }
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
            return false;
        }
    }
}
