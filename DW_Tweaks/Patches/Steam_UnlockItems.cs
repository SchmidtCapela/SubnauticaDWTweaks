using Harmony;
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
        public static readonly object methodHasItem = AccessTools.Method(typeof(SteamEconomy), "HasItem");

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
                if (
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    (codes[i + 1].opcode.Equals(OpCodes.Ldc_I4) || codes[i + 1].opcode.Equals(OpCodes.Ldc_I4_1)) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(methodHasItem) &&
                    codes[i + 3].opcode.Equals(OpCodes.Brfalse))
                {
                    numberUnlocked += 1;
                    codes.RemoveRange(i, 4);
                }
            }
            if (numberUnlocked == 0) Console.WriteLine("DW_Tweaks ERR: Failed to unlock any items in SteamEconomy_UnlockItems_patch.");
            return codes.AsEnumerable();
        }
    }
}
