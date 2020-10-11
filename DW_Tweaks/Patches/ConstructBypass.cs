using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.UpdatePlacement))]
    class BaseAddCellGhost_UpdatePlacement_patch
    {
        public static readonly object cellTypeField = AccessTools.Field(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.cellType));
        public static readonly object cellMinHeight = AccessTools.Field(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.minHeightFromTerrain));
        public static readonly object cellMaxHeight = AccessTools.Field(typeof(BaseAddCellGhost), nameof(BaseAddCellGhost.maxHeightFromTerrain));
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

        public static readonly float minHeight = 0f;
        public static readonly float maxHeight = 500f;

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected1 = false;
            bool injected2 = false;
            var labelLoadMinMax = generator.DefineLabel();
            var labelAfterMinMax = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 3; i < codes.Count - 3; i++)
            {
                if (!injected1 &&
                    Utils.isCode(codes[i    ], OpCodes.Ldfld, cellTypeField) &&
                    Utils.isCode(codes[i + 1], OpCodes.Ldc_I4_2) &&
                    Utils.isCode(codes[i + 2], OpCodes.Bne_Un) &&  // The test to see if the part is a foundation
                    Utils.isCode(codes[i - 3], OpCodes.Ldloc_0) &&
                    Utils.isCode(codes[i - 2], OpCodes.Brfalse))
                {
                    injected1 = true;
                    codes.InsertRange(i + 3, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                        new CodeInstruction(OpCodes.Call, funcKeyCode),
                        new CodeInstruction(OpCodes.Brtrue, codes[i - 2].operand),
                    });
                    if (injected2) break;
                }
                if (!injected2 &&
                    Utils.isCode(codes[i    ], OpCodes.Ldarg_0) &&
                    Utils.isCode(codes[i + 1], OpCodes.Ldfld, cellMinHeight) &&
                    Utils.isCode(codes[i + 2], OpCodes.Ldarg_0) &&
                    Utils.isCode(codes[i + 3], OpCodes.Ldfld, cellMaxHeight))
                {
                    injected2 = true;
                    codes[i].labels.Add(labelLoadMinMax);
                    codes[i + 4].labels.Add(labelAfterMinMax);
                    codes.InsertRange(i, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                        new CodeInstruction(OpCodes.Call, funcKeyCode),
                        new CodeInstruction(OpCodes.Brfalse, labelLoadMinMax),
                        new CodeInstruction(OpCodes.Ldc_R4, minHeight),
                        new CodeInstruction(OpCodes.Ldc_R4, maxHeight),
                        new CodeInstruction(OpCodes.Br, labelAfterMinMax),
                    });
                    if (injected1) break;
                }

            }
            if (!(injected1 && injected2)) Console.WriteLine("DW_Tweaks ERR: Failed to apply BaseAddCellGhost_UpdatePlacement_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(BaseAddCorridorGhost), nameof(BaseAddCorridorGhost.UpdatePlacement))]
    class BaseAddCorridorGhost_UpdatePlacement_patch
    {
        public static readonly object funcCheckCorridorConnection = AccessTools.Method(typeof(BaseAddCorridorGhost), nameof(BaseAddCorridorGhost.CheckCorridorConnection));
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Call) && codes[i].operand.Equals(funcCheckCorridorConnection) && codes[i + 1].opcode.Equals(OpCodes.Brtrue))
                {
                    injected = true;
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl));
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, funcKeyCode));
                    codes.Insert(i + 4, new CodeInstruction(OpCodes.Brtrue, codes[i + 1].operand));
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply BaseAddCorridorGhost_UpdatePlacement_patch.");
            return codes.AsEnumerable();
        }
    }

/*    [HarmonyPatch(typeof(BaseGhost), nameof(BaseGhost.PlaceWithBoundsCast))]
    class BaseGhost_PlaceWithBoundsCast_patch
    {
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode) });
        public static readonly object funcQuaternionLookRotation = AccessTools.Method(typeof(Quaternion), nameof(Quaternion.LookRotation), new Type[] { typeof(Vector3), typeof(Vector3) });
        public static readonly object funcQuaternionGetEulerAngles = AccessTools.Method(typeof(Quaternion), $"get_{nameof(Quaternion.eulerAngles)}", new Type[] { });
        public static readonly object funcQuaternionSetEulerAngles = AccessTools.Method(typeof(Quaternion), $"set_{nameof(Quaternion.eulerAngles)}", new Type[] { typeof(Vector3) });
        public static readonly object funcQuaternionEuler = AccessTools.Method(typeof(Quaternion), nameof(Quaternion.Euler), new Type[] { typeof(Vector3) });
        public static readonly object fieldVector3Y = AccessTools.Field(typeof(Vector3), nameof(Vector3.y));
        public static readonly object funcMathRound = AccessTools.Method(typeof(Math), nameof(Math.Round), new Type[] { typeof(double) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var notKeyPressed = generator.DefineLabel();

            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 4; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if (!injected &&
                    Utils.isCode(codes[i    ], OpCodes.Call, funcQuaternionLookRotation) &&
                    Utils.isCode(codes[i + 1], OpCodes.Stloc_3) &&
                    Utils.isCode(codes[i + 2], OpCodes.Ldloca_S) &&
                    Utils.isCode(codes[i + 3], OpCodes.Call) &&
                    Utils.isCode(codes[i + 4], OpCodes.Stloc_S))
                {
                    injected = true;
                    codes[i + 2].labels = new List<Label>() { notKeyPressed };
                    codes.InsertRange(i + 2, new List<CodeInstruction>() {
//                        new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftAlt),           // If pressing LeftAlt
//                        new CodeInstruction(OpCodes.Call, funcKeyCode),
//                        new CodeInstruction(OpCodes.Brfalse, notKeyPressed),
                        new CodeInstruction(OpCodes.Ldloca_S, Utils.getLocalVar(codes[i + 1])),                                // extents = orientation.eulerAngles
                        new CodeInstruction(OpCodes.Callvirt, funcQuaternionGetEulerAngles),
                        new CodeInstruction(OpCodes.Stloc_S, codes[i + 4].operand),
                        new CodeInstruction(OpCodes.Ldloca_S, codes[i + 4].operand),         // extents.y = round(extents.y / 15) * 15
                        new CodeInstruction(OpCodes.Ldloc_S, codes[i + 4].operand),
                        new CodeInstruction(OpCodes.Ldfld, fieldVector3Y),
                        new CodeInstruction(OpCodes.Ldc_R4, 15),
                        new CodeInstruction(OpCodes.Div),
                        new CodeInstruction(OpCodes.Conv_R8),
                        new CodeInstruction(OpCodes.Call, funcMathRound),
                        new CodeInstruction(OpCodes.Conv_R4),
                        new CodeInstruction(OpCodes.Ldc_R4, 15),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stfld, fieldVector3Y),
//                        new CodeInstruction(OpCodes.Ldloca_S, Utils.getLocalVar(codes[i + 1])),                                // orientation.eulerAngles = extents
//                        new CodeInstruction(OpCodes.Ldloc_S, codes[i + 4].operand),
//                        new CodeInstruction(OpCodes.Callvirt, funcQuaternionSetEulerAngles),
                        new CodeInstruction(OpCodes.Ldloc_S, codes[i + 4].operand),
                        new CodeInstruction(OpCodes.Call, funcQuaternionEuler),
                        new CodeInstruction(OpCodes.Stloc_S, Utils.getLocalVar(codes[i + 1])),                                // orientation.eulerAngles = extents
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply BaseGhost_PlaceWithBoundsCast_patch.");
            return codes.AsEnumerable();
        }
    }
*/
    [HarmonyPatch(typeof(Constructable), nameof(Constructable.CheckFlags))]
    class Constructable_CheckFlags_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static bool Prefix(ref bool __result)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.ValidateOutdoor))]
    class Builder_ValidateOutdoor_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static bool Prefix(ref bool __result)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
    /*
        [HarmonyPatch(typeof(Builder), nameof(Builder.CheckAsSubModule))]
        class Builder_CheckAsSubModule_Patch
        {
            // Test to see if using default values, skip patching if true
            public static bool Prepare()
            {
                return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
            }

            public static void Postfix(ref bool __result)
            {
                if (Input.GetKey(KeyCode.RightAlt)) __result = true;
            }
        }

        [HarmonyPatch(typeof(Builder), nameof(Builder.CheckSpace), new Type[] { typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(int), typeof(Collider)})]
        class Builder_CheckSpace_Patch
        {
            // Test to see if using default values, skip patching if true
            public static bool Prepare()
            {
                return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
            }

            public static void Postfix(ref bool __result)
            {
                if (Input.GetKey(KeyCode.RightAlt)) __result = true;
            }
        }
    */
    /*
        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.Place))]
        class PlaceTool_Place_Patch
        {
            public static readonly object fieldValidPosition = AccessTools.Field(typeof(PlaceTool), nameof(PlaceTool.validPosition));
            public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

            // Test to see if using default values, skip patching if true
            public static bool Prepare()
            {
                return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
            }

            public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
            {
                bool injected = false;
                var doPlace = generator.DefineLabel();
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 3; i++)
                {
                    if (!injected &&
                        Utils.isCode(codes[i    ], OpCodes.Ldarg_0) &&  // Usual test to see if the item can be built here
                        Utils.isCode(codes[i + 1], OpCodes.Ldfld, fieldValidPosition) &&
                        Utils.isCode(codes[i + 2], OpCodes.Brfalse))
                    {
                        injected = true;
                        codes[i + 3].labels.Add(doPlace);
                        codes.InsertRange(i, new List<CodeInstruction>() {
                            new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.RightAlt),
                            new CodeInstruction(OpCodes.Call, funcKeyCode),
                            new CodeInstruction(OpCodes.Brtrue, doPlace),
                        });
                        break;
                    }
                }
                if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply PlaceTool_Place_Patch.");
                return codes.AsEnumerable();
            }
        }
    */
    /*
    [HarmonyPatch(typeof(Builder), nameof(Builder.UpdateAllowed))]
    class Builder_UpdateAllowed_Patch
    {
        public static readonly object methodUpdateGhostModel = AccessTools.Method(typeof(Constructable), nameof(Constructable.UpdateGhostModel));
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var notKeyPressed = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 3; i < codes.Count - 3; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if (!injected &&
                    Utils.isCode(codes[i - 2], OpCodes.Ldloca_S) &&
                    Utils.isCode(codes[i - 1], OpCodes.Ldloc_2) &&
                    Utils.isCode(codes[i], OpCodes.Callvirt, methodUpdateGhostModel) &&
                    Utils.isCode(codes[i + 1], OpCodes.Stloc_1))
                {
                    injected = true;
                    codes[i + 2].labels.Add(notKeyPressed);
                    codes.InsertRange(i + 2, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl),
                        new CodeInstruction(OpCodes.Call, funcKeyCode),
                        new CodeInstruction(OpCodes.Brfalse, notKeyPressed),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Stloc_1),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Stloc_0),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Builder_UpdateAllowed_Patch.");
            return codes.AsEnumerable();
        }
    }
    */

    // Remove one of the bounding box tests, allowing items touching but not penetrating each other.
    [HarmonyPatch(typeof(Builder), nameof(Builder.GetObstacles))]
    class Builder_GetObstacles_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static bool Prefix(ref List<GameObject> results)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                results = new List<GameObject>();
                return false;
            }
            return true;
        }
    }
/*
    [HarmonyPatch(typeof(Builder), nameof(Builder.TryPlace))]
    class Builder_TryPlace_Patch
    {
        public static readonly object fieldAllowedOutside = AccessTools.Field(typeof(Builder), nameof(Builder.allowedOutside));

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Ldsfld) && codes[i].operand.Equals(fieldAllowedOutside) && codes[i + 1].opcode.Equals(OpCodes.Brfalse))
                {
                    // The test is redundant anyway, so remove it.
                    injected = true;
                    codes.RemoveRange(i, 2);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Builder_TryPlace_Patch.");
            return codes.AsEnumerable();
        }
    }
*/
/*
    // Seems to cause issues when reloading the area or loading the game.
    [HarmonyPatch(typeof(Planter), nameof(Planter.IsAllowedToAdd))]
    class Planter_IsAllowedToAdd_Patch  // Change planters so they dynamically adjust allowed plants
                                        // This is a hack, though; I'm just testing to see if the player is swimming.
    {
        public static readonly object planterGetContainerType = AccessTools.Method(typeof(Planter), nameof(Planter.GetContainerType));
        public static readonly object plantableAboveWater = AccessTools.Field(typeof(Plantable), nameof(Plantable.aboveWater));
        public static readonly object plantableUnderWater = AccessTools.Field(typeof(Plantable), nameof(Plantable.underwater));
        public static readonly object playerMain = AccessTools.Field(typeof(Player), nameof(Player.main));
        public static readonly object playerIsSwimming = AccessTools.Method(typeof(Player), nameof(Player.IsSwimming));

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var notInside = generator.DefineLabel();
            var notUnderwater = generator.DefineLabel();

            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 1; i < codes.Count - 17; i++)
            {
                // Console.WriteLine("DW_Tweaks Debug: {0:0000} {1}: ({2}){3}", i, codes[i].opcode, codes[i].opcode.OperandType, codes[i].operand);
                if (!injected &&
                    Utils.isCode(codes[i + 1], OpCodes.Call, planterGetContainerType) && // containerType = this.GetContainerType()
                    Utils.isCode(codes[i], OpCodes.Ldarg_0) &&
                    Utils.isCode(codes[i + 2], OpCodes.Stloc) &&
                    Utils.isCode(codes[i + 3], OpCodes.Ldloc) &&  // if (containerType != ItemsContainerType.LandPlants)
                    Utils.isCode(codes[i + 4], OpCodes.Ldc_I4_1) &&
                    Utils.isCode(codes[i + 5], OpCodes.Beq) &&
                    Utils.isCode(codes[i + 6], OpCodes.Ldloc) &&  // containerType == ItemsContainerType.WaterPlants
                    Utils.isCode(codes[i + 7], OpCodes.Ldc_I4_2) &&
                    Utils.isCode(codes[i + 8], OpCodes.Beq) &&
                    Utils.isCode(codes[i + 9], OpCodes.Br) &&
                    Utils.isCode(codes[i + 10], OpCodes.Ldloc_0) &&  // if (containerType == ItemsContainerType.LandPlants) return component.aboveWater
                    Utils.isCode(codes[i + 11], OpCodes.Ldfld) &&
                    Utils.isCode(codes[i + 12], OpCodes.Ret) &&
                    Utils.isCode(codes[i + 13], OpCodes.Ldloc_0) &&  // if (containerType == ItemsContainerType.WaterPlants) return component.underWater
                    Utils.isCode(codes[i + 14], OpCodes.Ldfld) &&
                    Utils.isCode(codes[i + 15], OpCodes.Ret) &&
                    Utils.isCode(codes[i + 16], OpCodes.Ldc_I4_0) &&  // Otherwise return false
                    Utils.isCode(codes[i + 17], OpCodes.Ret))
                {
                    injected = true;
                    List<Label> tempJump = codes[i].labels;  // There is a jump to this instruction
                    codes.RemoveRange(i, 18);
                    codes.InsertRange(i, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldsfld, playerMain) {labels = tempJump },  // Hack: use list of underwater plants if the player is swimming
                        new CodeInstruction(OpCodes.Callvirt, playerIsSwimming),
                        new CodeInstruction(OpCodes.Brtrue_S, notInside),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Ldfld, plantableAboveWater),
                        new CodeInstruction(OpCodes.Ret),
                        new CodeInstruction(OpCodes.Ldloc_0) {labels = new List<Label>() { notInside } },  // Hack: if not swimming, use land plants
                        new CodeInstruction(OpCodes.Ldfld, plantableUnderWater),
                        new CodeInstruction(OpCodes.Ret),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Planter_IsAllowedToAdd_Patch.");
            return codes.AsEnumerable();
        }
    }
*/
    // Tweak to the thermal generator to make it react to temperature decreases, needed to make it less cheaty.
    // Disabling it to test; it seems like storing the temperature is needed because the temperature simulation only works when the cell is loaded.
    /**[HarmonyPatch(typeof(ThermalPlant))]
    [HarmonyPatch("QueryTemperature")]
    class ThermalPlant_QueryTemperature_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static bool Prefix(ThermalPlant __instance)
        {
            WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
            if (main)
            {
                __instance.temperature = main.GetTemperature(__instance.transform.position);
                __instance.UpdateUI();
            }
            return false;
        }
    }**/

    // Make relay systems also check distance, needed for them to update on the Cyclops.
    [HarmonyPatch(typeof(PowerRelay), nameof(PowerRelay.MonitorCurrentConnection))]
    class PowerRelay_MonitorCurrentConnection_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static bool testDistance(PowerRelay relay)
        {
            Vector3 connectPoint = relay.GetConnectPoint();
            if ((relay.outboundRelay.GetConnectPoint(connectPoint) - connectPoint).sqrMagnitude < (relay.maxOutboundDistance * relay.maxOutboundDistance))
                return true;
            else return false;
        }

        public static readonly MethodInfo funcValidRelay = AccessTools.Method(typeof(PowerRelay), nameof(PowerRelay.IsValidRelayForConnection));
        public static readonly MethodInfo funcTestDistance = AccessTools.Method(typeof(PowerRelay_MonitorCurrentConnection_Patch), nameof(PowerRelay_MonitorCurrentConnection_Patch.testDistance));

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var labelDisconnect = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&  // The test to see if the outbound connection is still valid
                    codes[i].opcode.Equals(OpCodes.Call) && codes[i].operand.Equals(funcValidRelay) &&
                    codes[i + 1].opcode.Equals(OpCodes.Brtrue))
                {
                    injected = true;
                    codes[i + 2].labels.Add(labelDisconnect);  // Here the disconnection case is handled
                    codes.InsertRange(i + 1, new List<CodeInstruction>() {  // Also test the distance
                        new CodeInstruction(OpCodes.Brfalse, labelDisconnect),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, funcTestDistance),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply PowerRelay_MonitorCurrentConnection_Patch.");
            return codes.AsEnumerable();
        }
    }
}
