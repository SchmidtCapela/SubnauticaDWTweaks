using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SteamEconomy))]
    [HarmonyPatch("UnlockItems")]
    class SteamEconomy_UnlockItems_patch
    {
        public static readonly object methodHasItem = AccessTools.Method(typeof(EconomyItems), "HasItem");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.UnlockSteamInventoryItems;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int numberUnlocked = 0;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 4; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if (
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldstr) &&
                    codes[i + 3].opcode.Equals(OpCodes.Callvirt) && codes[i + 3].operand.Equals(methodHasItem) &&
                    codes[i + 4].opcode.Equals(OpCodes.Brfalse))
                {
                    // Console.WriteLine("DW_Tweaks Inf: Found and unlocked item {0}.", codes[i + 2].operand);
                    numberUnlocked += 1;
                    codes.RemoveRange(i, 5);
                }
            }
            if (numberUnlocked == 0) Console.WriteLine("DW_Tweaks ERR: Failed to unlock any items in SteamEconomy_UnlockItems_patch.");
            return codes.AsEnumerable();
        }
    }
}
