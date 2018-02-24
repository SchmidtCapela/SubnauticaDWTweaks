using Harmony;
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
        public static readonly object methodGetDeltaTime = AccessTools.Method(typeof(Time), "get_deltaTime");
        public static readonly object methodMinFloat = AccessTools.Method(typeof(Mathf), "Min", new Type[] { typeof(float), typeof(float) });
        public static readonly object fieldSoundEmitter = AccessTools.Field(typeof(FireExtinguisher), "soundEmitter");
        public static readonly object methodSoundEmitterPlay = AccessTools.Method(typeof(FMOD_CustomEmitter), "Play");
        public static readonly object methodSoundEmitterStop = AccessTools.Method(typeof(FMOD_CustomEmitter), "Stop");

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
                    codes[i].opcode.Equals(OpCodes.Ldfld) && codes[i].operand.Equals(fieldSoundEmitter) &&
                    codes[i + 1].opcode.Equals(OpCodes.Callvirt) && codes[i + 1].operand.Equals(methodSoundEmitterPlay) &&
                    codes[i + 2].opcode.Equals(OpCodes.Br) &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldfld) && codes[i + 4].operand.Equals(fieldSoundEmitter) &&
                    codes[i + 5].opcode.Equals(OpCodes.Callvirt) && codes[i + 5].operand.Equals(methodSoundEmitterStop) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldarg_0))
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
