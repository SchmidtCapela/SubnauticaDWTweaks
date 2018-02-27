using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SubControl))]
    [HarmonyPatch("FixedUpdate")]
    class SubControl_FixedUpdate_patch
    {
        public static readonly FieldInfo fieldBaseTurningTorque = AccessTools.Field(typeof(SubControl), "BaseTurningTorque");
        public static readonly FieldInfo funcBaseForwardAccel = AccessTools.Field(typeof(SubControl), "BaseForwardAccel");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.CyclopsSpeedMult == 1 && DW_Tweaks_Settings.Instance.CyclopsTurningMult == 1) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injectedSpeed = false;
            bool injectedTurning = false;
            var labelGoingForward = generator.DefineLabel();
            var labelForwardBackwardEnd = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!injectedSpeed &&  // The whole math expression goes here
                    codes[i].opcode.Equals(OpCodes.Ldfld) && codes[i].operand.Equals(funcBaseForwardAccel))
                {
                    injectedSpeed = true;
                    codes.InsertRange(i + 1, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_R4, DW_Tweaks_Settings.Instance.CyclopsSpeedMult),
                        new CodeInstruction(OpCodes.Mul),
                    });
                    if (injectedSpeed && injectedTurning) break;
                }
                if (!injectedTurning &&  // The whole math expression goes here
                    codes[i].opcode.Equals(OpCodes.Ldfld) && codes[i].operand.Equals(fieldBaseTurningTorque))
                {
                    injectedTurning = true;
                    codes.InsertRange(i + 1, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_R4, DW_Tweaks_Settings.Instance.CyclopsTurningMult),
                        new CodeInstruction(OpCodes.Mul),
                    });
                    if (injectedSpeed && injectedTurning) break;
                }
            }
            if (!(injectedSpeed && injectedTurning)) Console.WriteLine("DW_Tweaks ERR: Failed to apply SubControl_FixedUpdate_patch.");
            return codes.AsEnumerable();
        }
    }
}
