using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(IngameMenu))]
    [HarmonyPatch("UpdateButtons")]
    class IngameMenu_UpdateButtons_patch
    {
        public static readonly object methodLanguageGet = AccessTools.Method(typeof(Language), "Get");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.HardcoreEnableSave;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 6; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldc_I4_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Callvirt) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ret))
                {
                    injected = true;
                    codes[i].opcode = OpCodes.Ldc_I4_1;  // Make the save button active
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply IngameMenu_UpdateButtons_patch.");
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("OnKill")]
    class Player_OnKill_Patch  // Disables the save delete on death
    {
        public static readonly object SaveClearSlotAsync = AccessTools.Method(typeof(SaveLoadManager), "ClearSlotAsync");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.HardcoreEnableSave;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 1; i < codes.Count - 5; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Call) &&  // Get static class SaveLoadManager
                    codes[i + 1].opcode.Equals(OpCodes.Call) &&  // Get static class SaveLoadManager
                    codes[i + 2].opcode.Equals(OpCodes.Callvirt) &&  // GetCurrentSlot
                    codes[i + 3].opcode.Equals(OpCodes.Callvirt) && codes[i + 3].operand.Equals(SaveClearSlotAsync) &&
                    codes[i + 4].opcode.Equals(OpCodes.Pop))  // Discards the return value
                {
                    injected = true;
                    codes.RemoveRange(i, 5);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Player_OnKill_patch.");
            return codes.AsEnumerable();
        }
    }
}
