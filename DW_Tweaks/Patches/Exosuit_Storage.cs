using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

// Note: there are two constants defined in the Exosuit class, storageWidth and storageHeight, which don't seem to be used anywhere.

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("IsAllowedToRemove")]
    class Exosuit_IsAllowedToRemove_patch
    {
        public static readonly OpCode[] valueMap = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8
        };

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.ExosuitStorageWidth == 6) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Ldc_I4_6) && codes[i + 1].opcode.Equals(OpCodes.Ldc_I4_1))
                {
                    injected = true;
                    codes[i].opcode = valueMap[DW_Tweaks_Settings.Instance.ExosuitStorageWidth];
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Exosuit_IsAllowedToRemove_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("UpdateStorageSize")]
    class Exosuit_UpdateStorageSize_patch
    {
        public static readonly OpCode[] valueMap = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8
        };

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.ExosuitStorageWidth == 6) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Ldc_I4_6) && codes[i + 1].opcode.Equals(OpCodes.Ldloc_0))
                {
                    injected = true;
                    codes[i].opcode = valueMap[DW_Tweaks_Settings.Instance.ExosuitStorageWidth];
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Exosuit_UpdateStorageSize_patch.");
            return codes.AsEnumerable();
        }
    }
}
