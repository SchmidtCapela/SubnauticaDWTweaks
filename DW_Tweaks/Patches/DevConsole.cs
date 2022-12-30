using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(DevConsole), nameof(DevConsole.Submit))]
    class DevConsole_Submit_patch
    {
        public static readonly object fieldHasUsedConsole = AccessTools.Field(typeof(DevConsole), nameof(DevConsole.hasUsedConsole));

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.ConsoleDoesntDisableAchievements;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_I4_1) &&  // True
                    codes[i + 2].opcode.Equals(OpCodes.Stfld) && codes[i + 2].operand.Equals(fieldHasUsedConsole))  // store in this.hasUsedConsole
                {
                    injected = true;
                    codes[i + 1].opcode = OpCodes.Ldc_I4_0;  // Change to False
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply DevConsole_Submit_patch.");
            return codes.AsEnumerable();
        }
    }
}
