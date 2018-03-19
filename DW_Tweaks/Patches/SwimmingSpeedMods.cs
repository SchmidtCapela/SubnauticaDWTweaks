using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(UnderwaterMotor))]
    [HarmonyPatch("AlterMaxSpeed")]
    class UnderwaterMotor_AlterMaxSpeed_patch
    {

        public static readonly MethodInfo funcGetHeldTool = AccessTools.Method(typeof(Inventory), "GetHeldTool");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            if (DW_Tweaks_Settings.Instance.speedTank == 0.425f &&                 // Tank
                DW_Tweaks_Settings.Instance.speedDoubleTank == 0.5f &&             // High Capacity Tank
                DW_Tweaks_Settings.Instance.speedPlasteelTank == 0.10625f &&       // Lightweight Tank
                DW_Tweaks_Settings.Instance.speedHighCapacityTank == 0.6375f &&    // Ultra High Capacity Tank
                DW_Tweaks_Settings.Instance.speedHighCapacityTankInv == 1.275f &&  // Ultra High Capacity Tank in inventory
                DW_Tweaks_Settings.Instance.speedReinforcedDiveSuit == 1f &&       // Reinforced Dive Suit, body only
                DW_Tweaks_Settings.Instance.speedFins == 1.5f &&                   // Fins
                DW_Tweaks_Settings.Instance.speedUltraGlideFins == 2.5f &&         // Ultraglide Fins
                DW_Tweaks_Settings.Instance.speedSwimChargeFins == 0f &&           // Swim Charge Fins
                DW_Tweaks_Settings.Instance.speedHeldTool == 1f                    // When holding a tool
            ) return false;
            else return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            int testTank = -1;
            int testDoubleTank = -1;
            int testPlasteelTank = -1;
            int testHighCapacityTank = -1;
            bool testHighCapacityTankInv = false;
            int testReinforcedDiveSuit = -1;
            int testFins = -1;
            int testUltraGlideFins = -1;
            bool testHeldTool = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 2; i < codes.Count - 8; i++)
            {
                // Set of tests to catch the switch statements
                if (testTank < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.Tank) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testTank = i;
                if (testDoubleTank < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.DoubleTank) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testDoubleTank = i;
                if (testPlasteelTank < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.PlasteelTank) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testPlasteelTank = i;
                if (testHighCapacityTank < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.HighCapacityTank) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testHighCapacityTank = i;
                if (testReinforcedDiveSuit < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.ReinforcedDiveSuit) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testReinforcedDiveSuit = i;
                if (testFins < 0 &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.Fins) &&
                    codes[i].opcode.Equals(OpCodes.Beq))
                    testFins = i;
                // This one also adds the extra test for the Swim Charge Fins
                if (testUltraGlideFins < 0 &&
                    codes[i - 2].opcode.Equals(OpCodes.Ldloc_S) &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.UltraGlideFins) &&
                    codes[i].opcode.Equals(OpCodes.Beq) &&
                    codes[i + 1].opcode.Equals(OpCodes.Br)
                    )
                {
                    testUltraGlideFins = i;
                    if (DW_Tweaks_Settings.Instance.speedSwimChargeFins != 0f)
                    {
                        codes.InsertRange(i + 1, new List<CodeInstruction>() {
                            new CodeInstruction(OpCodes.Ldloc_S, codes[i - 2].operand),
                            new CodeInstruction(OpCodes.Ldc_I4, (int)TechType.SwimChargeFins),
                            new CodeInstruction(OpCodes.Bne_Un, codes[i + 1].operand),
                            new CodeInstruction(OpCodes.Ldloc_0),
                            new CodeInstruction(OpCodes.Ldc_R4, DW_Tweaks_Settings.Instance.speedSwimChargeFins),
                            new CodeInstruction(OpCodes.Add),
                            new CodeInstruction(OpCodes.Stloc_0),
                        });
                    }
                }
                // Catch the targets of the Switch statements
                if (testTank > 0 &&
                    codes[i].labels.Contains((Label)codes[testTank].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Sub))
                {
                    testTank = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedTank;
                }
                if (testDoubleTank > 0 &&
                    codes[i].labels.Contains((Label)codes[testDoubleTank].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Sub))
                {
                    testDoubleTank = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedDoubleTank;
                }
                if (testPlasteelTank > 0 &&
                    codes[i].labels.Contains((Label)codes[testPlasteelTank].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Sub))
                {
                    testPlasteelTank = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedPlasteelTank;
                }
                if (testHighCapacityTank > 0 &&
                    codes[i].labels.Contains((Label)codes[testHighCapacityTank].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Sub))
                {
                    testHighCapacityTank = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedHighCapacityTank;
                }
                if (testFins > 0 &&
                    codes[i].labels.Contains((Label)codes[testFins].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Add))
                {
                    testFins = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedFins;
                }
                if (testUltraGlideFins > 0 &&
                    codes[i].labels.Contains((Label)codes[testUltraGlideFins].operand) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Add))
                {
                    testUltraGlideFins = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedUltraGlideFins;
                }
                // The segment that applies the reinforced suit speed penalty is different
                if (testReinforcedDiveSuit > 0 &&
                    codes[i - 1].labels.Contains((Label)codes[testReinforcedDiveSuit].operand) &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_R4) &&
                    codes[i + 2].opcode.Equals(OpCodes.Sub))
                {
                    testReinforcedDiveSuit = 0;
                    codes[i + 1].operand = DW_Tweaks_Settings.Instance.speedReinforcedDiveSuit;
                }
                // The tank in inventory and held tool are more conventional
                if (!testHighCapacityTankInv &&
                    codes[i - 1].opcode.Equals(OpCodes.Ldc_I4) && codes[i - 1].operand.Equals((int)TechType.HighCapacityTank) &&
                    codes[i].opcode.Equals(OpCodes.Callvirt) && // GetCount
                    codes[i + 1].opcode.Equals(OpCodes.Stloc_S) &&  // Store the count
                    codes[i + 2].opcode.Equals(OpCodes.Ldloc_0) &&  // Load current speed
                    codes[i + 3].opcode.Equals(OpCodes.Ldloc_S) &&  // Load the count
                    codes[i + 4].opcode.Equals(OpCodes.Conv_R4) &&  // As a float
                    codes[i + 5].opcode.Equals(OpCodes.Ldc_R4) &&  // Speed penalty
                    codes[i + 6].opcode.Equals(OpCodes.Mul) &&  // Multiplies the penalty by the number of tanks in the inventory
                    codes[i + 7].opcode.Equals(OpCodes.Sub))
                {
                    testHighCapacityTankInv = true;
                    codes[i + 5].operand = DW_Tweaks_Settings.Instance.speedHighCapacityTankInv;
                }
                if (!testHeldTool &&
                    codes[i - 1].opcode.Equals(OpCodes.Callvirt) && codes[i - 1].operand.Equals(funcGetHeldTool) &&
                    codes[i].opcode.Equals(OpCodes.Ldnull) && // Test to see if null
                    codes[i + 1].opcode.Equals(OpCodes.Call) &&  // op_Equality
                    codes[i + 2].opcode.Equals(OpCodes.Stloc_S) &&
                    codes[i + 3].opcode.Equals(OpCodes.Ldloc_S) &&
                    codes[i + 4].opcode.Equals(OpCodes.Brfalse) &&
                    codes[i + 5].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 6].opcode.Equals(OpCodes.Ldc_R4) &&  // Speed Penalty
                    codes[i + 7].opcode.Equals(OpCodes.Add))
                {
                    testHeldTool = true;
                    codes[i + 6].operand = DW_Tweaks_Settings.Instance.speedHeldTool;
                }
                if (testHighCapacityTankInv && testHeldTool &&
                    testTank == 0 && testDoubleTank == 0 && testPlasteelTank == 0 && testHighCapacityTank == 0 && testReinforcedDiveSuit == 0 && testFins == 0 && testUltraGlideFins == 0)
                    break;
            }
            if (!testHighCapacityTankInv)    Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for ultra high capacity tank in inventory in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (!testHeldTool)               Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for tool being held in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testTank != 0)               Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for tank in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testDoubleTank != 0)         Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for high capacity tank in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testPlasteelTank != 0)       Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for lightweight tank in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testHighCapacityTank != 0)   Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for ultra high capacity tank in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testReinforcedDiveSuit != 0) Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for reinforced dive suit in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testFins != 0)               Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for fins in UnderwaterMotor_AlterMaxSpeed_patch.");
            if (testUltraGlideFins != 0)     Console.WriteLine("DW_Tweaks ERR: Failed to apply patch for ultraglide fins in UnderwaterMotor_AlterMaxSpeed_patch.");
            return codes.AsEnumerable();
        }
    }
}
