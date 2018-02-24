using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(BaseAddCellGhost))]
    [HarmonyPatch("UpdatePlacement")]
    class BaseAddCellGhost_UpdatePlacement_patch
    {
        public static readonly object cellTypeField = AccessTools.Field(typeof(BaseAddCellGhost), "cellType");
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.BypassBuildRestrictions;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 3; i < codes.Count - 2; i++)
            {
                if (!injected && codes[i].opcode.Equals(OpCodes.Ldfld) && codes[i].operand.Equals(cellTypeField) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldc_I4_2) && codes[i + 2].opcode.Equals(OpCodes.Bne_Un) &&
                    codes[i - 3].opcode.Equals(OpCodes.Ldloc_0) && codes[i - 2].opcode.Equals(OpCodes.Brfalse))
                {
                    injected = true;
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_I4, (int)KeyCode.LeftControl));
                    codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, funcKeyCode));
                    codes.Insert(i + 5, new CodeInstruction(OpCodes.Brtrue, codes[i - 2].operand));
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply BaseAddCellGhost_UpdatePlacement_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(BaseAddCorridorGhost))]
    [HarmonyPatch("UpdatePlacement")]
    class BaseAddCorridorGhost_UpdatePlacement_patch
    {
        public static readonly object funcCheckCorridorConnection = AccessTools.Method(typeof(BaseAddCorridorGhost), "CheckCorridorConnection");
        public static readonly object funcKeyCode = AccessTools.Method(typeof(Input), "GetKey", new Type[] { typeof(KeyCode) });

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

    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch("CheckFlags")]
    class Constructable_CheckFlags_Patch
    {
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
}
