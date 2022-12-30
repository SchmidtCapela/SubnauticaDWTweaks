using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
/*    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch("Start")]
    class ResourceTracker_Start_patch
    {
        public static readonly object methodCanScan = AccessTools.Method(typeof(PDAScanner), "CanScan", new Type[] { typeof(GameObject) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.FragmentTrackAll;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 11; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldc_I4) && codes[i + 2].operand.Equals((Int32)TechType.Fragment) &&
                    codes[i + 3].opcode.Equals(OpCodes.Bne_Un) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 5].opcode.Equals(OpCodes.Call) &&
                    codes[i + 6].opcode.Equals(OpCodes.Call) && codes[i + 6].operand.Equals(methodCanScan) &&
                    codes[i + 7].opcode.Equals(OpCodes.Ldc_I4_0) &&
                    codes[i + 8].opcode.Equals(OpCodes.Ceq) &&
                    (codes[i + 9].opcode.Equals(OpCodes.Br_S) || codes[i + 9].opcode.Equals(OpCodes.Br)) &&
                    codes[i + 10].opcode.Equals(OpCodes.Ldc_I4_0) &&
                    codes[i + 11].opcode.Equals(OpCodes.Stloc_0))
                {
                    injected = true;
                    List<CodeInstruction> replacement = new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_I4_0) {labels = codes[i].labels },
                        new CodeInstruction(OpCodes.Stloc_0),
                    };
                    codes.RemoveRange(i, 12);
                    codes.InsertRange(i, replacement);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply ResourceTracker_Start_patch.");
            return codes.AsEnumerable();
        }
    }
*/
    // This function only serves to remove fragments from tracking as the player completes recipes.
    [HarmonyPatch(typeof(ResourceTracker))]
    [HarmonyPatch("OnBlueprintHandTargetUsed")]
    class ResourceTracker_UpdateFragments_patch
    {

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.FragmentTrackAll;
        }

        public static bool Prefix(ResourceTracker __instance)
        {
            __instance.Unregister();
            return false;
        }
    }
}
