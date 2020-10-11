using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    // Cyclops
    [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
    [HarmonyPatch("Update")]
    class CyclopsHelmHUDManager_Update_patch
    {
        public static readonly object funcCeilToInt = AccessTools.Method(typeof(Mathf), "CeilToInt");
        public static readonly object fieldSubRoot = AccessTools.Field(typeof(CyclopsHelmHUDManager), "subRoot");
        public static readonly object funcGetTemperature = AccessTools.Method(typeof(SubRoot), "GetTemperature");
        public static readonly object fieldThermalReactorCharge = AccessTools.Field(typeof(SubRoot), "thermalReactorCharge");
        public static readonly object fieldPowerRelay = AccessTools.Field(typeof(SubRoot), "powerRelay");
        public static readonly object funcRelayPower = AccessTools.Method(typeof(PowerRelay), "GetPower");
        public static readonly object funcRelayMaxPower = AccessTools.Method(typeof(PowerRelay), "GetMaxPower");
        public static readonly object funcAnimationCurveEvaluate = AccessTools.Method(typeof(AnimationCurve), "Evaluate");
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.VehicleHUDExtraPrecision;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int CodeLoc1 = -1;
            int CodeLoc2 = -1;
            var valueCtrl = generator.DefineLabel();
            var valueCtrlShift = generator.DefineLabel();
            var valueEnd = generator.DefineLabel();
            var formatCtrl = generator.DefineLabel();
            var formatCtrlShift = generator.DefineLabel();
            var formatEnd = generator.DefineLabel();

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 4; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if ((CodeLoc1 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldSubRoot) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldfld) && codes[i + 2].operand.Equals(fieldPowerRelay) &&
                    codes[i + 3].opcode.Equals(OpCodes.Callvirt) && codes[i + 3].operand.Equals(funcRelayPower) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 5].opcode.Equals(OpCodes.Ldfld) && codes[i + 5].operand.Equals(fieldSubRoot) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldfld) && codes[i + 6].operand.Equals(fieldPowerRelay) &&
                    codes[i + 7].opcode.Equals(OpCodes.Callvirt) && codes[i + 7].operand.Equals(funcRelayMaxPower) &&
                    codes[i + 8].opcode.Equals(OpCodes.Div) &&
                    codes[i + 9].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 9].operand.Equals((float)100) &&
                    codes[i + 10].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 11].opcode.Equals(OpCodes.Call) && codes[i + 11].operand.Equals(funcCeilToInt) &&
                    codes[i + 12].opcode.Equals(OpCodes.Stloc_2))
                {
                    // Console.WriteLine("DW_Tweaks Info: Found Cyclops HUD energy variable.");
                    CodeLoc1 = i;
                }
                if ((CodeLoc1 > 0) && (CodeLoc2 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldstr) && codes[i].operand.Equals("{0}%") &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldloc_2))
                {
                    // Console.WriteLine("DW_Tweaks Info: Found Cyclops HUD energy string.");
                    CodeLoc2 = i;
                    break;
                }
            }
            if ((CodeLoc1 >= 0) && (CodeLoc2 >= 0))
            {
                // Change the format string first, otherwise the other location is invalidated
                codes.RemoveAt(CodeLoc2);
                // Label for the first instruction after assigning the format string
                codes[CodeLoc2].labels.Add(formatEnd);
                codes.InsertRange(CodeLoc2, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, formatCtrl),
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}%"), // changed default string
                    new CodeInstruction(OpCodes.Br, formatEnd),
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftShift) { labels = new List<Label>() { formatCtrl } },
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, formatCtrlShift),
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}C"),  // temperature when LeftControl pressed
                    new CodeInstruction(OpCodes.Br, formatEnd),
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.000}") { labels = new List<Label>() { formatCtrlShift } },  // Power regen when LeftControl+LeftShift pressed
                });

                // Now to change the displayed value
                codes.RemoveRange(CodeLoc1, 11);
                // Label for the first instruction that calls the CeilToInt function
                codes[CodeLoc1].labels.Add(valueEnd);
                codes.InsertRange(CodeLoc1, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, valueCtrl),
                    // Default: power value, though multiplied by 1000
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldSubRoot),
                    new CodeInstruction(OpCodes.Ldfld, fieldPowerRelay),
                    new CodeInstruction(OpCodes.Callvirt, funcRelayPower),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldSubRoot),
                    new CodeInstruction(OpCodes.Ldfld, fieldPowerRelay),
                    new CodeInstruction(OpCodes.Callvirt, funcRelayMaxPower),
                    new CodeInstruction(OpCodes.Div),
                    new CodeInstruction(OpCodes.Ldc_R4, 1000f),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Br, valueEnd),
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftShift) { labels = new List<Label>() { valueCtrl } },
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, valueCtrlShift),
                    // LeftControl: Temperature
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldSubRoot),
                    new CodeInstruction(OpCodes.Call, funcGetTemperature),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)10),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Br, valueEnd),
                    // LeftControl+LeftShift: Power Regen
                    new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>() { valueCtrlShift } },
                    new CodeInstruction(OpCodes.Ldfld, fieldSubRoot),
                    new CodeInstruction(OpCodes.Ldfld, fieldThermalReactorCharge),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldSubRoot),
                    new CodeInstruction(OpCodes.Call, funcGetTemperature),
                    new CodeInstruction(OpCodes.Callvirt, funcAnimationCurveEvaluate),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)1.5),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)1000),
                    new CodeInstruction(OpCodes.Mul),
                });
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply CyclopsHelmHUDManager_Update_patch.");
            return codes.AsEnumerable();
        }
    }
    
    // PRAWN
    [HarmonyPatch(typeof(uGUI_ExosuitHUD))]
    [HarmonyPatch("Update")]
    class uGUI_ExosuitHUD_Update_patch
    {
        public static readonly object funcCeilToInt = AccessTools.Method(typeof(Mathf), "CeilToInt");
        public static readonly object funcStringFormat1 = AccessTools.Method(typeof(String), "Format", new Type[] { typeof(string), typeof(object) });
        public static readonly object funcGetHUDValues = AccessTools.Method(typeof(Exosuit), "GetHUDValues");
        public static readonly object fieldLastPower = AccessTools.Field(typeof(uGUI_ExosuitHUD), "lastPower");
        public static readonly object funcGetStringforInt = AccessTools.Method(typeof(IntStringCache), "GetStringForInt");
        public static readonly object funcGetTemperature = AccessTools.Method(typeof(Vehicle), "GetTemperature");
        public static readonly object fieldThermalReactorCharge = AccessTools.Field(typeof(Exosuit), "thermalReactorCharge");
        public static readonly object funcAnimationCurveEvaluate = AccessTools.Method(typeof(AnimationCurve), "Evaluate");
        public static readonly object fieldTemperatureSmoothValue = AccessTools.Field(typeof(uGUI_ExosuitHUD), "temperatureSmoothValue");
        public static readonly object fieldLastTemperature = AccessTools.Field(typeof(uGUI_ExosuitHUD), "lastTemperature");
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.VehicleHUDExtraPrecision;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int CodeLoc1 = -1;
            int CodeLoc2 = -1;
            int CodeLoc3 = -1;
            int CodeLoc4 = -1;
            LocalVariableInfo localCurrEnergy = null;
            LocalVariableInfo localCurrEnergyInt = null;
            LocalVariableInfo localTemperature = null;
            var valueCtrl = generator.DefineLabel();
            var valueEnd = generator.DefineLabel();
            var formatCtrl = generator.DefineLabel();
            var formatEnd = generator.DefineLabel();

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                // Find where it's storing the energy value
                if ((localCurrEnergy == null) &&
                    codes[i].opcode.Equals(OpCodes.Callvirt) && codes[i].operand.Equals(funcGetHUDValues) &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldloca_S) && codes[i - 2].opcode.Equals(OpCodes.Ldloca_S))
                {
                    localCurrEnergy = (LocalVariableInfo)codes[i - 2].operand;
                }
                // Find where it's storing the temperature value
                if ((localTemperature == null) &&
                    codes[i].opcode.Equals(OpCodes.Callvirt) && codes[i].operand.Equals(funcGetTemperature) &&
                    codes[i + 1].opcode.Equals(OpCodes.Stloc_S))
                {
                    localTemperature = (LocalVariableInfo)codes[i + 1].operand;
                }
                // First patch point: energy value
                if ((localCurrEnergy != null) && (CodeLoc1 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_S) && codes[i].operand.Equals(localCurrEnergy) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 1].operand.Equals((float)100) &&
                    codes[i + 2].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 3].opcode.Equals(OpCodes.Call) && codes[i + 3].operand.Equals(funcCeilToInt) &&
                    codes[i + 4].opcode.Equals(OpCodes.Stloc_S))
                {
                    CodeLoc1 = i;
                    localCurrEnergyInt = (LocalVariableInfo)codes[i + 4].operand;
                }
                // Second patch point: energy string
                if ((CodeLoc1 > 0) && (CodeLoc2 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldLastPower) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(funcGetStringforInt))
                {
                    CodeLoc2 = i;
                }
                // Third patch point: temperature value
                if ((CodeLoc3 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 2].operand.Equals((float)-10000) &&
                    codes[i + 3].opcode.Equals(OpCodes.Blt) &&
                    codes[i + 12].opcode.Equals(OpCodes.Ldloc_S) &&
                    codes[i + 13].opcode.Equals(OpCodes.Stfld) && codes[i + 13].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 14].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 15].opcode.Equals(OpCodes.Ldfld) && codes[i + 15].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 16].opcode.Equals(OpCodes.Call) && codes[i + 16].operand.Equals(funcCeilToInt) &&
                    codes[i + 17].opcode.Equals(OpCodes.Stloc_S))
                {
                    CodeLoc3 = i;
                }
                // Third patch point: temperature string
                if ((CodeLoc3 > 0) && (CodeLoc4 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldLastTemperature) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(funcGetStringforInt))
                {
                    CodeLoc4 = i;
                }
            }
            // Changing from last to first
            if ((CodeLoc3 >= 0) && (CodeLoc4 >= 0) && (CodeLoc3 >= CodeLoc1))
            {
                // Change the format string first, otherwise the other location is invalidated
                // Delete old function
                codes.RemoveRange(CodeLoc4, 3);
                codes.InsertRange(CodeLoc4, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}"),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldLastTemperature),
                    new CodeInstruction(OpCodes.Box, typeof(Int32)),
                    new CodeInstruction(OpCodes.Call, funcStringFormat1),
                });
                codes.InsertRange(CodeLoc3 + 16, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_R4, (float)10),
                    new CodeInstruction(OpCodes.Mul),
                });
                codes.RemoveRange(CodeLoc3, 12);
            }
            else {
                if ((CodeLoc3 >= 0) && (CodeLoc4 >= 0)) Console.WriteLine("DW_Tweaks ERR: Failed to apply temperature uGUI_ExosuitHUD_Update_patch, the order is wrong.");
                else Console.WriteLine("DW_Tweaks ERR: Failed to apply temperature uGUI_ExosuitHUD_Update_patch.");
            }
            if ((CodeLoc1 >= 0) && (CodeLoc2 >= 0))
            {
                // Change the format string first, otherwise the other location is invalidated
                // Delete old function
                codes.RemoveRange(CodeLoc2, 3);
                codes.InsertRange(CodeLoc2, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, formatCtrl), // Need to assign the label when adding the target instruction
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}"),
                    new CodeInstruction(OpCodes.Br, formatEnd),
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.000}") {labels = new List<Label>() { formatCtrl } },
                    new CodeInstruction(OpCodes.Ldloc_S, localCurrEnergyInt) {labels = new List<Label>() { formatEnd } },
                    new CodeInstruction(OpCodes.Box, typeof(Int32)),
                    new CodeInstruction(OpCodes.Call, funcStringFormat1),
                });

                List<Label> tempJump = codes[CodeLoc1].labels;  // There is a jump to this instruction
                codes.RemoveRange(CodeLoc1, 2);
                codes.InsertRange(CodeLoc1, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl) {labels = tempJump },  // Restaure the missing jump target to prevent an unnamed exception
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, valueCtrl), // Need to assign the label when adding the target instruction
                    new CodeInstruction(OpCodes.Ldloc_S, localCurrEnergy),
                    new CodeInstruction(OpCodes.Br, valueEnd),
                    new CodeInstruction(OpCodes.Ldloc_0) {labels = new List<Label>() { valueCtrl } },
                    new CodeInstruction(OpCodes.Ldfld, fieldThermalReactorCharge),
                    new CodeInstruction(OpCodes.Ldloc_S, localTemperature),
                    new CodeInstruction(OpCodes.Callvirt, funcAnimationCurveEvaluate),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)1000) {labels = new List<Label>() { valueEnd } },
                });
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply energy uGUI_ExosuitHUD_Update_patch.");
            return codes.AsEnumerable();
        }
    }

    // Seamoth
    [HarmonyPatch(typeof(uGUI_SeamothHUD))]
    [HarmonyPatch("Update")]
    class uGUI_SeamothHUD_Update_patch
    {
        public static readonly object funcCeilToInt = AccessTools.Method(typeof(Mathf), "CeilToInt");
        public static readonly object funcStringFormat1 = AccessTools.Method(typeof(String), "Format", new Type[] { typeof(string), typeof(object) });
        public static readonly object funcGetHUDValues = AccessTools.Method(typeof(SeaMoth), "GetHUDValues");
        public static readonly object fieldLastPower = AccessTools.Field(typeof(uGUI_SeamothHUD), "lastPower");
        public static readonly object funcGetStringforInt = AccessTools.Method(typeof(IntStringCache), "GetStringForInt");
        public static readonly object funcGetTemperature = AccessTools.Method(typeof(Vehicle), "GetTemperature");
        public static readonly object fieldTemperatureSmoothValue = AccessTools.Field(typeof(uGUI_SeamothHUD), "temperatureSmoothValue");
        public static readonly object fieldLastTemperature = AccessTools.Field(typeof(uGUI_SeamothHUD), "lastTemperature");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.VehicleHUDExtraPrecision;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int CodeLoc1 = -1;
            int CodeLoc2 = -1;
            int CodeLoc3 = -1;
            int CodeLoc4 = -1;
            LocalVariableInfo localCurrEnergy = null;
            LocalVariableInfo localCurrEnergyInt = null;
            LocalVariableInfo localTemperature = null;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                // Find where it's storing the energy value
                if ((localCurrEnergy == null) &&
                    codes[i].opcode.Equals(OpCodes.Callvirt) && codes[i].operand.Equals(funcGetHUDValues) &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldloca_S))
                {
                    localCurrEnergy = (LocalVariableInfo)codes[i - 1].operand;
                }
                // Find where it's storing the temperature value
                if ((localTemperature == null) &&
                    codes[i].opcode.Equals(OpCodes.Callvirt) && codes[i].operand.Equals(funcGetTemperature) &&
                    codes[i + 1].opcode.Equals(OpCodes.Stloc_S))
                {
                    localTemperature = (LocalVariableInfo)codes[i + 1].operand;
                }
                // First patch point: energy value
                if ((localCurrEnergy != null) && (CodeLoc1 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_S) && codes[i].operand.Equals(localCurrEnergy) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 1].operand.Equals((float)100) &&
                    codes[i + 2].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 3].opcode.Equals(OpCodes.Call) && codes[i + 3].operand.Equals(funcCeilToInt) &&
                    codes[i + 4].opcode.Equals(OpCodes.Stloc_S))
                {
                    CodeLoc1 = i;
                    localCurrEnergyInt = (LocalVariableInfo)codes[i + 4].operand;
                }
                // Second patch point: energy string
                if ((CodeLoc1 > 0) && (CodeLoc2 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldLastPower) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(funcGetStringforInt))
                {
                    CodeLoc2 = i;
                }
                // Third patch point: temperature value
                if ((CodeLoc3 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 2].operand.Equals((float)-10000) &&
                    codes[i + 3].opcode.Equals(OpCodes.Blt) &&
                    codes[i + 12].opcode.Equals(OpCodes.Ldloc_S) &&
                    codes[i + 13].opcode.Equals(OpCodes.Stfld) && codes[i + 13].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 14].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 15].opcode.Equals(OpCodes.Ldfld) && codes[i + 15].operand.Equals(fieldTemperatureSmoothValue) &&
                    codes[i + 16].opcode.Equals(OpCodes.Call) && codes[i + 16].operand.Equals(funcCeilToInt) &&
                    codes[i + 17].opcode.Equals(OpCodes.Stloc_S))
                {
                    // Console.WriteLine("DW_Tweaks Info: Found temperature calc.");
                    CodeLoc3 = i;
                }
                // Third patch point: temperature string
                if ((CodeLoc3 > 0) && (CodeLoc4 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldLastTemperature) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(funcGetStringforInt))
                {
                    // Console.WriteLine("DW_Tweaks Info: Found temperature string.");
                    CodeLoc4 = i;
                }
            }
            // Changing from last to first
            if ((CodeLoc3 >= 0) && (CodeLoc4 >= 0) && (CodeLoc3 >= CodeLoc1))
            {
                // Change the format string first, otherwise the other location is invalidated
                // Delete old function
                codes.RemoveRange(CodeLoc4, 3);
                codes.InsertRange(CodeLoc4, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}"),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldLastTemperature),
                    new CodeInstruction(OpCodes.Box, typeof(Int32)),
                    new CodeInstruction(OpCodes.Call, funcStringFormat1),
                });
                codes.InsertRange(CodeLoc3 + 16, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_R4, (float)10),
                    new CodeInstruction(OpCodes.Mul),
                });
                codes.RemoveRange(CodeLoc3, 12);  // Remove the reference to SmoothDamp to remove lag in temperature reporting
            }
            else
            {
                if ((CodeLoc3 >= 0) && (CodeLoc4 >= 0)) Console.WriteLine("DW_Tweaks ERR: Failed to apply temperature uGUI_SeamothHUD_Update_patch, the order is wrong.");
                else Console.WriteLine("DW_Tweaks ERR: Failed to apply temperature uGUI_SeamothHUD_Update_patch.");
            }
            if ((CodeLoc1 >= 0) && (CodeLoc2 >= 0))
            {
                // Change the format string first, otherwise the other location is invalidated
                // Delete old function
                codes.RemoveRange(CodeLoc2, 3);
                codes.InsertRange(CodeLoc2, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}"),
                    new CodeInstruction(OpCodes.Ldloc_S, localCurrEnergyInt),
                    new CodeInstruction(OpCodes.Box, typeof(Int32)),
                    new CodeInstruction(OpCodes.Call, funcStringFormat1),
                });

                codes[CodeLoc1 + 1].operand = (float)1000;
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply energy uGUI_SeamothHUD_Update_patch.");
            return codes.AsEnumerable();
        }
    }

    // Enable displaying current and total energy on the Cyclops as if it was a base
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("IsPowerEnabled")]
    class uGUI_PowerIndicator_IsPowerEnabled_patch
    {
        public static readonly object fieldIsBase = AccessTools.Field(typeof(SubRoot), "isBase");
        public static readonly object fieldPlayerMain = AccessTools.Field(typeof(Player), "main");
        public static readonly object fieldPlayerMode = AccessTools.Field(typeof(Player), "mode");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.VehicleHUDExtraPrecision;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool inject = false;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!inject &&
                    (codes[i].opcode.Equals(OpCodes.Ldloc_0) || codes[i].opcode.Equals(OpCodes.Ldloc_1) || codes[i].opcode.Equals(OpCodes.Ldloc_2) || codes[i].opcode.Equals(OpCodes.Ldloc_3) || codes[i].opcode.Equals(OpCodes.Ldloc_S)) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldIsBase) &&
                    codes[i + 2].opcode.Equals(OpCodes.Brfalse))
                {
                    inject = true;
                    codes.RemoveRange(i, 3);
                    break;
                }
            }
            if (!inject) Console.WriteLine("DW_Tweaks ERR: Failed to apply energy uGUI_PowerIndicator_IsPowerEnabled_patch.");
            return codes.AsEnumerable();
        }
    }
}
