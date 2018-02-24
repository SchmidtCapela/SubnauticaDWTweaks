using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleChange")]
    class SeaMoth_OnUpgradeModuleChange_patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.SeamothDepthMod3 == 700) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldc_I4) && codes[i].operand.Equals((int)TechType.VehicleHullModule3) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 1].operand.Equals((float)700) &&
                    codes[i + 2].opcode.Equals(OpCodes.Callvirt))
                {
                    injected = true;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.SeamothDepthMod3;
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply SeaMoth_OnUpgradeModuleChange_patch.");
            return codes.AsEnumerable();
        }
    }
}
