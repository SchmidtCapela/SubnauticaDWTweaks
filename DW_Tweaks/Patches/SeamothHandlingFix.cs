﻿using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("ApplyPhysicsMove")]
    class Vehicle_ApplyPhysicsMove_patch
    {
        /**public static readonly MethodInfo funcPlaySplashSound = AccessTools.Method(typeof(Vehicle), "PlaySplashSound");
        public static readonly Type typeVehicleAccelerationModifier = AccessTools.Inner(typeof(Vehicle), "VehicleAccelerationModifier");
        public static readonly FieldInfo fieldWasAboveWater = AccessTools.Field(typeof(Vehicle), "wasAboveWater");
        public static readonly FieldInfo fieldAccelerationModifiers = AccessTools.Field(typeof(Vehicle), "accelerationModifiers");**/

        public static readonly FieldInfo fieldX = AccessTools.Field(typeof(Vector3), "x");
        public static readonly FieldInfo fieldY = AccessTools.Field(typeof(Vector3), "y");
        public static readonly FieldInfo fieldZ = AccessTools.Field(typeof(Vector3), "z");
        public static readonly MethodInfo funcClampMagnitude = AccessTools.Method(typeof(Vector3), "ClampMagnitude");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.SeamothHandlingFix > 0);
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var labelGoingForward = generator.DefineLabel();
            var labelForwardBackwardEnd = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 2; i < codes.Count - 40; i++)
            {
                if (!injected &&  // The whole math expression goes here
                    codes[i - 2].opcode.Equals(OpCodes.Ldloca_S) && // Opperand is the vector a
                    codes[i - 1].opcode.Equals(OpCodes.Call) && // Vector3.Normalize
                    codes[i].opcode.Equals(OpCodes.Ldloca_S) && // Opperand is the vector a
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldX) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && // Mathf.Abs
                    codes[i + 3].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ldfld) &&  // SidewardForce
                    codes[i + 5].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 6].operand.Equals(0f) &&
                    codes[i + 7].opcode.Equals(OpCodes.Ldloca_S) &&
                    codes[i + 8].opcode.Equals(OpCodes.Ldfld) && codes[i + 8].operand.Equals(fieldZ) &&
                    codes[i + 9].opcode.Equals(OpCodes.Call) &&  // Mathf.Max
                    codes[i + 10].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 11].opcode.Equals(OpCodes.Ldfld) &&  // ForwardForce
                    codes[i + 12].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 13].opcode.Equals(OpCodes.Add) &&
                    codes[i + 14].opcode.Equals(OpCodes.Ldc_R4) && codes[i + 14].operand.Equals(0f) &&
                    codes[i + 15].opcode.Equals(OpCodes.Ldloca_S) &&
                    codes[i + 16].opcode.Equals(OpCodes.Ldfld) && codes[i + 16].operand.Equals(fieldZ) &&
                    codes[i + 17].opcode.Equals(OpCodes.Neg) &&
                    codes[i + 18].opcode.Equals(OpCodes.Call) &&  // Max
                    codes[i + 19].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 20].opcode.Equals(OpCodes.Ldfld) &&  // BackwardForce
                    codes[i + 21].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 22].opcode.Equals(OpCodes.Add) &&
                    codes[i + 23].opcode.Equals(OpCodes.Ldloca_S) &&
                    codes[i + 24].opcode.Equals(OpCodes.Ldfld) && codes[i + 24].operand.Equals(fieldY) &&
                    codes[i + 25].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 26].opcode.Equals(OpCodes.Ldfld) && // VerticalForce
                    codes[i + 27].opcode.Equals(OpCodes.Mul) &&
                    codes[i + 28].opcode.Equals(OpCodes.Call) &&  // Mathf.Abs
                    codes[i + 29].opcode.Equals(OpCodes.Add) &&
                    codes[i + 30].opcode.Equals(OpCodes.Stloc_S) &&  // Opperand is float d
                    codes[i + 31].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 32].opcode.Equals(OpCodes.Call) &&  // get_transform
                    codes[i + 33].opcode.Equals(OpCodes.Callvirt) &&  // get_rotation
                    codes[i + 34].opcode.Equals(OpCodes.Ldloc_S) &&
                    codes[i + 35].opcode.Equals(OpCodes.Ldloc_3) &&
                    codes[i + 36].opcode.Equals(OpCodes.Call) &&  // Vector3 op_Multiply(float32, Vector3)
                    codes[i + 37].opcode.Equals(OpCodes.Call) &&  // Vector3 op_Multiply(Quaternion, Vector3)
                    codes[i + 38].opcode.Equals(OpCodes.Call) &&  // get_deltatime
                    codes[i + 39].opcode.Equals(OpCodes.Call) &&  // Vector3 op_Multiply(Vector3, float32)
                    codes[i + 40].opcode.Equals(OpCodes.Stloc_S)) //  Opperand is vector force
                {
                    injected = true;
                    List<CodeInstruction> replacement = new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldloc_S, codes[i].operand),  // Vector a
                        new CodeInstruction(OpCodes.Ldc_R4, 1f),  // Will clamp to 1
                        new CodeInstruction(OpCodes.Call, funcClampMagnitude),  // ClampMagnitude
                        new CodeInstruction(OpCodes.Stloc_S, codes[i].operand),  // Vector a
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),  // To store X
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),
                        new CodeInstruction(OpCodes.Ldfld, fieldX),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 4].operand), // SidewardForce
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stfld, fieldX),  // Store multiplied X force
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),  // To store Y
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),
                        new CodeInstruction(OpCodes.Ldfld, fieldY),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 26].operand), // VerticalForce
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stfld, fieldY),  // Store multiplied Y force
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),  // To store Z
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),
                        new CodeInstruction(OpCodes.Ldfld, fieldZ),
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i].operand),  // Test if Z>0, AKA going forward
                        new CodeInstruction(OpCodes.Ldfld, fieldZ),
                        new CodeInstruction(OpCodes.Ldc_R4, 0f),
                        new CodeInstruction(OpCodes.Bge, labelGoingForward),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 20].operand), // BackwardForce
                        new CodeInstruction(OpCodes.Br, labelForwardBackwardEnd),
                        new CodeInstruction(OpCodes.Ldarg_0) {labels = new List<Label>() { labelGoingForward } },
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 11].operand), // ForwardForce
                        new CodeInstruction(OpCodes.Mul) {labels = new List<Label>() { labelForwardBackwardEnd } },
                        new CodeInstruction(OpCodes.Stfld, fieldZ),  // Store multiplied Z force
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(codes[i + 32]),  // get_transform
                        new CodeInstruction(codes[i + 33]),  // get_rotation
                        new CodeInstruction(OpCodes.Ldc_R4, DW_Tweaks_Settings.Instance.SeamothHandlingFix),  // Make it so the forward velocity is equal to the previous exploit velocity
                        new CodeInstruction(codes[i + 35]),  // Vector a
                        new CodeInstruction(codes[i + 36]),  // Vector3 op_Multiply(float32, Vector3)
                        new CodeInstruction(codes[i + 37]),  // Vector3 op_Multiply(Quaternion, Vector3)
                        new CodeInstruction(codes[i + 38]),  // get_deltatime
                        new CodeInstruction(codes[i + 39]),  // Vector3 op_Multiply(Vector3, float32)
                        new CodeInstruction(codes[i + 40]),  // Store in variable force
                    };
                    codes.RemoveRange(i - 2, 43);
                    codes.InsertRange(i - 2, replacement);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Vehicle_ApplyPhysicsMove_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Update")]
    class SeaMoth_Update_patch
    {
        public static readonly MethodInfo funcGetMoveDirection = AccessTools.Method(typeof(GameInput), "GetMoveDirection");
        public static readonly MethodInfo funcClampMagnitude = AccessTools.Method(typeof(Vector3), "ClampMagnitude");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.SeamothHandlingFix > 0);
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var labelGoingForward = generator.DefineLabel();
            var labelForwardBackwardEnd = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (!injected &&  // The whole math expression goes here
                    codes[i].opcode.Equals(OpCodes.Call) && codes[i].operand.Equals(funcGetMoveDirection)) //  Opperand is vector force
                {
                    injected = true;
                    codes.InsertRange(i + 1, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_R4, 1f),  // Will clamp to 1
                        new CodeInstruction(OpCodes.Call, funcClampMagnitude),  // ClampMagnitude
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply SeaMoth_Update_patch.");
            return codes.AsEnumerable();
        }
    }
}