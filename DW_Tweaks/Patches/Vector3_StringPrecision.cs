using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Vector3))]
    [HarmonyPatch("ToString")]
    [HarmonyPatch(new Type[] { })]
    class Vector3_ToString_patch
    {

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.Vector3StringPrecision == 1) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Ldstr) && codes[i].operand.Equals("({0:F1}, {1:F1}, {2:F1})"))
                {
                    injected = true;
                    codes[i].operand = "({0:F"+ DW_Tweaks_Settings.Instance.Vector3StringPrecision + "}, {1:F"+ DW_Tweaks_Settings.Instance.Vector3StringPrecision+"}, {2:F"+ DW_Tweaks_Settings.Instance.Vector3StringPrecision+"})";
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Vector3_ToString_patch.");
            return codes.AsEnumerable();
        }
    }
}
