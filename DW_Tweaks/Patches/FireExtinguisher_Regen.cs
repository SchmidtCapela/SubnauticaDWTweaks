using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(FireExtinguisher))]
    [HarmonyPatch("Update")]
    class FireExtinguisher_Update_patch
    {
        public static readonly object fieldFuel = AccessTools.Field(typeof(FireExtinguisher), "fuel");
        public static readonly object fieldUsedThisFrame = AccessTools.Field(typeof(FireExtinguisher), "usedThisFrame");
        public static readonly object methodGetDeltaTime = AccessTools.Method(typeof(Time), "get_deltaTime");
        public static readonly object methodMinFloat = AccessTools.Method(typeof(Mathf), "Min", new Type[] { typeof(float), typeof(float) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.FireExtinguisherRegen == 0) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 6; i++)
            {
                if (!injected &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldc_I4_0) &&
                    codes[i + 5].opcode.Equals(OpCodes.Stfld) && codes[i + 5].operand.Equals(fieldUsedThisFrame) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldsfld))
                {
                    injected = true;
                    codes.InsertRange(i + 6, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, fieldFuel),
                        new CodeInstruction(OpCodes.Ldc_R4, DW_Tweaks_Settings.Instance.FireExtinguisherRegen),
                        new CodeInstruction(OpCodes.Call, methodGetDeltaTime),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldc_R4, (float)100),
                        new CodeInstruction(OpCodes.Call, methodMinFloat),
                        new CodeInstruction(OpCodes.Stfld, fieldFuel),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply FireExtinguisher_Update_patch.");
            return codes.AsEnumerable();
        }
    }
}
