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
    [HarmonyPatch("OnSelect")]
    class IngameMenu_OnSelect_patch
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
                    codes[i].opcode.Equals(OpCodes.Ldstr) && codes[i].operand.Equals("SaveAndQuitToMainMenu") &&
                    codes[i + 1].opcode.Equals(OpCodes.Callvirt) && codes[i + 1].operand.Equals(methodLanguageGet) &&
                    codes[i + 2].opcode.Equals(OpCodes.Callvirt) &&  // set_text
                    codes[i + 3].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldfld) &&  // saveButton
                    codes[i + 5].opcode.Equals(OpCodes.Callvirt) &&  // get_gameObject
                    codes[i + 6].opcode.Equals(OpCodes.Ldc_I4_0) &&  // False for the SetActive
                    codes[i + 7].opcode.Equals(OpCodes.Callvirt) &&  // SetActive
                    codes[i + 8].opcode.Equals(OpCodes.Br) &&  // End of the conditional
                    codes[i + 9].opcode.Equals(OpCodes.Ldarg_0) &&  // 5 instructions to copy before the branch at i+8
                    codes[i + 10].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 11].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 12].opcode.Equals(OpCodes.Call) &&
                    codes[i + 13].opcode.Equals(OpCodes.Callvirt))
                {
                    injected = true;
                    codes[i + 6].opcode = OpCodes.Ldc_I4_1;  // Make the save button active
                    // Copy the code that enables and disables the Save button
                    codes.InsertRange(i + 8, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 10].operand),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, codes[i + 12].operand),
                        new CodeInstruction(OpCodes.Callvirt, codes[i + 13].operand),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply IngameMenu_OnSelect_patch.");
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
