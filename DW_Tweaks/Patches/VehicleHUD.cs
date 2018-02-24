using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
    [HarmonyPatch("Update")]
    class CyclopsHelmHUDManager_Update_patch
    {
        public static readonly object funcCeilToInt = AccessTools.Method(typeof(Mathf), "CeilToInt");
        public static readonly object fieldPowerText = AccessTools.Field(typeof(CyclopsHelmHUDManager), "powerText");
        public static readonly object fieldSubRoot = AccessTools.Field(typeof(CyclopsHelmHUDManager), "subRoot");
        public static readonly object funcGetTemperature = AccessTools.Method(typeof(SubRoot), "GetTemperature");
        public static readonly object fieldThermalReactorCharge = AccessTools.Field(typeof(SubRoot), "thermalReactorCharge");
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
            var localCurrEnergy = original.GetMethodBody().LocalVariables[4];
            var valueCtrl = generator.DefineLabel();
            var valueCtrlShift = generator.DefineLabel();
            var valueEnd = generator.DefineLabel();
            var formatCtrl = generator.DefineLabel();
            var formatCtrlShift = generator.DefineLabel();
            var formatEnd = generator.DefineLabel();

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 4; i++)
            {
                //Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if ((CodeLoc1 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_3) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 1].operand.Equals((float)100) &&
                    codes[i + 2].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 3].opcode.Equals(OpCodes.Call) && codes[i + 3].operand.Equals(funcCeilToInt) &&
                    //codes[i + 4].opcode.Equals(OpCodes.Stloc_S) && codes[i + 4].operand.Equals(localCurrEnergy))
                    codes[i + 4].opcode.Equals(OpCodes.Stloc_S))
                {
                    CodeLoc1 = i;
                    localCurrEnergy = (LocalVariableInfo)codes[i + 4].operand;
                }
                if ((CodeLoc1 > 0) && (CodeLoc2 < 0) &&
                    codes[i].opcode.Equals(OpCodes.Ldstr) && codes[i].operand.Equals("{0}%") &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldloc_S) && codes[i + 1].operand.Equals(localCurrEnergy))
                {
                    CodeLoc2 = i;
                    break;
                }
            }
            if ((CodeLoc1 >= 0) && (CodeLoc2 >= 0))
            {
                // Change the format string first, otherwise the other location is invalidated
                codes.Insert(CodeLoc2, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl));
                codes.Insert(CodeLoc2 + 1, new CodeInstruction(OpCodes.Call, funcKeyCode));
                codes.Insert(CodeLoc2 + 2, new CodeInstruction(OpCodes.Brtrue, formatCtrl)); // Need to assign the label when adding the target instruction
                // Change the default string; the instruction is already OpCodes.Ldstr
                codes[CodeLoc2 + 3].operand = "{0:0\\.0}%";
                codes.Insert(CodeLoc2 + 4, new CodeInstruction(OpCodes.Br, formatEnd)); // Label assigned at the end
                codes.Insert(CodeLoc2 + 5, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftShift));
                codes[CodeLoc2 + 5].labels.Add(formatCtrl);
                codes.Insert(CodeLoc2 + 6, new CodeInstruction(OpCodes.Call, funcKeyCode));
                codes.Insert(CodeLoc2 + 7, new CodeInstruction(OpCodes.Brtrue, formatCtrlShift)); // Need to assign the label when adding the target instruction
                codes.Insert(CodeLoc2 + 8, new CodeInstruction(OpCodes.Ldstr, "{0:0\\.0}C"));
                codes.Insert(CodeLoc2 + 9, new CodeInstruction(OpCodes.Br, formatEnd)); // Label assigned at the end
                codes.Insert(CodeLoc2 + 10, new CodeInstruction(OpCodes.Ldstr, "{0:0\\.000}"));
                codes[CodeLoc2 + 10].labels.Add(formatCtrlShift);
                // Label for the instruction after assigning the format string
                codes[CodeLoc2 + 11].labels.Add(formatEnd);

                // Now to change the displayed value
                codes.Insert(CodeLoc1, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl));
                codes.Insert(CodeLoc1 + 1, new CodeInstruction(OpCodes.Call, funcKeyCode));
                codes.Insert(CodeLoc1 + 2, new CodeInstruction(OpCodes.Brtrue, valueCtrl)); // Need to assign the label when adding the target instruction
                // The next three instructions are kept, though the multiplier is changed
                codes[CodeLoc1 + 4].operand = (float)1000;
                codes.Insert(CodeLoc1 + 6, new CodeInstruction(OpCodes.Br, valueEnd)); // Label assigned at the end
                // The other value possibilities
                codes.Insert(CodeLoc1 + 7, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftShift));
                codes[CodeLoc1 + 7].labels.Add(valueCtrl);
                codes.Insert(CodeLoc1 + 8, new CodeInstruction(OpCodes.Call, funcKeyCode));
                codes.Insert(CodeLoc1 + 9, new CodeInstruction(OpCodes.Brtrue, valueCtrlShift)); // Need to assign the label when adding the target instruction
                codes.Insert(CodeLoc1 + 10, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(CodeLoc1 + 11, new CodeInstruction(OpCodes.Ldfld, fieldSubRoot));
                codes.Insert(CodeLoc1 + 12, new CodeInstruction(OpCodes.Call, funcGetTemperature));
                codes.Insert(CodeLoc1 + 13, new CodeInstruction(OpCodes.Ldc_R4, (float)10));
                codes.Insert(CodeLoc1 + 14, new CodeInstruction(OpCodes.Mul));
                codes.Insert(CodeLoc1 + 15, new CodeInstruction(OpCodes.Br, valueEnd)); // Label assigned at the end
                codes.Insert(CodeLoc1 + 16, new CodeInstruction(OpCodes.Ldarg_0));
                codes[CodeLoc1 + 16].labels.Add(valueCtrlShift);
                codes.Insert(CodeLoc1 + 17, new CodeInstruction(OpCodes.Ldfld, fieldSubRoot));
                codes.Insert(CodeLoc1 + 18, new CodeInstruction(OpCodes.Ldfld, fieldThermalReactorCharge));
                codes.Insert(CodeLoc1 + 19, new CodeInstruction(OpCodes.Ldarg_0));
                codes.Insert(CodeLoc1 + 20, new CodeInstruction(OpCodes.Ldfld, fieldSubRoot));
                codes.Insert(CodeLoc1 + 21, new CodeInstruction(OpCodes.Call, funcGetTemperature));
                codes.Insert(CodeLoc1 + 22, new CodeInstruction(OpCodes.Callvirt, funcAnimationCurveEvaluate));
                codes.Insert(CodeLoc1 + 23, new CodeInstruction(OpCodes.Ldc_R4, (float)1.5));
                codes.Insert(CodeLoc1 + 24, new CodeInstruction(OpCodes.Mul));
                codes.Insert(CodeLoc1 + 25, new CodeInstruction(OpCodes.Ldc_R4, (float)1000));
                codes.Insert(CodeLoc1 + 26, new CodeInstruction(OpCodes.Mul));
                // Label for the instruction that calls the CeilToInt function
                codes[CodeLoc1 + 27].labels.Add(valueEnd);
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply CyclopsHelmHUDManager_Update_patch.");
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
            //}
            return codes.AsEnumerable();
        }
    }
    
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
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
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
                    codes[i + 3].opcode.Equals(OpCodes.Bge_Un) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldloc_S) &&
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
                codes.RemoveRange(CodeLoc3 + 5, 8);
                codes.RemoveRange(CodeLoc3, 4);
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
                    new CodeInstruction(OpCodes.Ldstr, "{0:0\\.000}"),
                    new CodeInstruction(OpCodes.Ldloc_S, localCurrEnergyInt),
                    new CodeInstruction(OpCodes.Box, typeof(Int32)),
                    new CodeInstruction(OpCodes.Call, funcStringFormat1),
                });
                codes[CodeLoc2 + 5].labels.Add(formatCtrl);
                codes[CodeLoc2 + 6].labels.Add(formatEnd);

                List<Label> tempJump = codes[CodeLoc1].labels;  // There is a jump to this instruction
                codes.RemoveRange(CodeLoc1, 2);
                codes.InsertRange(CodeLoc1, new List<CodeInstruction>() {
                    new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                    new CodeInstruction(OpCodes.Call, funcKeyCode),
                    new CodeInstruction(OpCodes.Brtrue, valueCtrl), // Need to assign the label when adding the target instruction
                    new CodeInstruction(OpCodes.Ldloc_S, localCurrEnergy),
                    new CodeInstruction(OpCodes.Br, valueEnd),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldThermalReactorCharge),
                    new CodeInstruction(OpCodes.Ldloc_S, localTemperature),
                    new CodeInstruction(OpCodes.Callvirt, funcAnimationCurveEvaluate),
                    new CodeInstruction(OpCodes.Ldc_R4, (float)1000),
                });
                codes[CodeLoc1].labels = tempJump;  // Restaure the missing jump target to prevent an unnamed exception
                codes[CodeLoc1 + 5].labels.Add(valueCtrl);
                codes[CodeLoc1 + 9].labels.Add(valueEnd);
            }
            else Console.WriteLine("DW_Tweaks ERR: Failed to apply energy uGUI_ExosuitHUD_Update_patch.");
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
            //}
            return codes.AsEnumerable();
        }
    }

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
                    codes[i + 3].opcode.Equals(OpCodes.Bge_Un) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldloc_S) &&
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
                codes.RemoveRange(CodeLoc3 + 5, 8);
                codes.RemoveRange(CodeLoc3, 4);
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
            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
            //}
            return codes.AsEnumerable();
        }
    }
}
