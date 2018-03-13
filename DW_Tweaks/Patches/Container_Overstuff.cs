using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(ItemsContainer))]
    [HarmonyPatch("HasRoomFor")]
    [HarmonyPatch(new Type[] { typeof(Pickupable) })]
    class ItemsContainer_HasRoomFor_patch
    {

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.ContainerOverstuff > 1);
        }

        public static bool Prefix(ItemsContainer __instance, Pickupable pickupable, ref bool __result)
        {
            Vector2int itemSize = CraftData.GetItemSize(pickupable.GetTechType());
            if (itemSize.x == 1 && itemSize.y == 1)
            {
                List<TechType> techTypes = __instance.GetItemTypes();
                if (techTypes.Count() == 1 && techTypes[0] == pickupable.GetTechType() && __instance.count < (__instance.sizeX * __instance.sizeY * DW_Tweaks_Settings.Instance.ContainerOverstuff))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    // Disable the overflow warning for the specific situation allowed by the mod
    [HarmonyPatch(typeof(ItemsContainer))]
    [HarmonyPatch("Sort")]
    class ItemsContainer_Sort_patch
    {

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.ContainerOverstuff > 1);
        }

        public static bool testContainer(ItemsContainer container)
        {
            List<TechType> techTypes = container.GetItemTypes();
            if (techTypes.Count() == 1)
            {
                Vector2int itemSize = CraftData.GetItemSize(techTypes[0]);
                if (itemSize.x == 1 && itemSize.y == 1 && container.count <= (container.sizeX * container.sizeY * DW_Tweaks_Settings.Instance.ContainerOverstuff))
                {
                    return true;
                }
            }
            return false;
        }

        public static readonly MethodInfo funcTrySort = AccessTools.Method(typeof(ItemsContainer), "TrySort");
        public static readonly MethodInfo funcTestContainer = AccessTools.Method(typeof(ItemsContainer_Sort_patch), "testContainer");

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var labelGoingForward = generator.DefineLabel();
            var labelForwardBackwardEnd = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Call) && codes[i].operand.Equals(funcTrySort) &&
                    codes[i+1].opcode.Equals(OpCodes.Brtrue))
                {
                    injected = true;
                    codes.InsertRange(i + 2, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, funcTestContainer),
                        new CodeInstruction(OpCodes.Brtrue, codes[i+1].operand),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply ItemsContainer_Sort_patch.");
            return codes.AsEnumerable();
        }
    }
}
