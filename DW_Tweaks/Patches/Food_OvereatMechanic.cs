using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Eat")]
    class Survival_Eat_patch
    {
        public static readonly object fieldFood = AccessTools.Field(typeof(Survival), "food");
        public static readonly object methodMaxFloat = AccessTools.Method(typeof(Mathf), "Max", new Type[] { typeof(float), typeof(float) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.FoodOvereatAlternateRule;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int loc1 = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 11; i++)
            {
                if ((loc1 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldfld) && codes[i].operand.Equals(fieldFood) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&  // Limit above which no nutrition is gained
                    codes[i + 2].opcode.Equals(OpCodes.Bgt_Un) &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 5].opcode.Equals(OpCodes.Ldfld) && codes[i + 5].operand.Equals(fieldFood) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldloc_1) &&
                    codes[i + 7].opcode.Equals(OpCodes.Callvirt) &&  // GetFoodValue
                    codes[i + 8].opcode.Equals(OpCodes.Add) &&
                    codes[i + 9].opcode.Equals(OpCodes.Ldc_R4) &&  codes[i + 9].operand.Equals((float)0) &&
                    codes[i + 10].opcode.Equals(OpCodes.Ldc_R4) &&  // MaxOvereat
                    codes[i + 11].opcode.Equals(OpCodes.Call))  // Clamp
                {
                    loc1 = i;
                    break;
                }
            }
            if (loc1 >= 0)
            {
                List<CodeInstruction> replacement = new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldFood),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)0),
                    new CodeInstruction(OpCodes.Ldc_R4, codes[loc1 + 1].operand),
                    new CodeInstruction(OpCodes.Call, codes[loc1 + 11].operand),  // Clamp
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Callvirt, codes[loc1 + 7].operand),  // GetFoodValue
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldFood),
                    new CodeInstruction(OpCodes.Call, methodMaxFloat),
                };
                codes.RemoveRange(loc1, 12);
                codes.InsertRange(loc1, replacement);
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply Survival_Eat_patch.");
            return codes.AsEnumerable();
        }
    }
}
