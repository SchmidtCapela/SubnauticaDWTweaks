using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("UpdateHunger")]
    class Survival_UpdateHunger_patch
    {
        public static readonly object fieldUpdateInterval = AccessTools.Field(typeof(Survival), "kUpdateHungerInterval");
        public static readonly object fieldDayNightCycleMain = AccessTools.Field(typeof(DayNightCycle), "main");
        public static readonly object methodGetDayNightSpeed = AccessTools.Method(typeof(DayNightCycle), "get_dayNightSpeed");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.ScaleWithDayNightSpeed;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int count = 0;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldUpdateInterval))
                {
                    ++count;
                    codes.InsertRange(i + 2, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldsfld, fieldDayNightCycleMain),
                        new CodeInstruction(OpCodes.Callvirt, methodGetDayNightSpeed),
                        new CodeInstruction(OpCodes.Mul),
                    });
                }
            }
            if (count < 2) Console.WriteLine(string.Format("DW_Tweaks ERR: Patch applied {0} times in Survival_UpdateHunger_patch.", count));
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch("UpdateScanning")]
    class MapRoomFunctionality_UpdateScanning_patch
    {
        public static readonly object fieldPowerRelay = AccessTools.Field(typeof(MapRoomFunctionality), "powerRelay");
        public static readonly object MethodConsumeEnergy = AccessTools.Method(typeof(PowerSystem), "ConsumeEnergy");
        public static readonly object fieldDayNightCycleMain = AccessTools.Field(typeof(DayNightCycle), "main");
        public static readonly object methodGetDayNightSpeed = AccessTools.Method(typeof(DayNightCycle), "get_dayNightSpeed");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.ScaleWithDayNightSpeed;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldPowerRelay) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 2].operand.Equals(0.5f) &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldloca_S) &&
                    codes[i + 4].opcode.Equals(OpCodes.Call) && codes[i + 4].operand.Equals(MethodConsumeEnergy))
                {
                    injected = true;
                    codes.InsertRange(i + 3, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldsfld, fieldDayNightCycleMain),
                        new CodeInstruction(OpCodes.Callvirt, methodGetDayNightSpeed),
                        new CodeInstruction(OpCodes.Mul),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply MapRoomFunctionality_UpdateScanning_patch.");
            return codes.AsEnumerable();
        }
    }
}
