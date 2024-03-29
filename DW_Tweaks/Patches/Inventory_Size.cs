﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("Awake")]
    class Inventory_Awake_patch
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
            if ((DW_Tweaks_Settings.Instance.InventoryWidth == 6) && (DW_Tweaks_Settings.Instance.InventoryHeight == 8)) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldc_I4_6) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_I4_8) &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldstr) && codes[i + 3].operand.Equals("InventoryLabel"))
                {
                    injected = true;
                    codes[i].opcode = valueMap[DW_Tweaks_Settings.Instance.InventoryWidth];
                    codes[i + 1].opcode = valueMap[DW_Tweaks_Settings.Instance.InventoryHeight];
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Inventory_Awake_patch.");
            return codes.AsEnumerable();
        }
    }
}
