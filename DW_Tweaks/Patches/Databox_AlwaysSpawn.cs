using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(DataboxSpawner))]
    [HarmonyPatch("Start")]
    class DataboxSpawner_Start_patch
    {
        public static readonly object methodKnownTechContains = AccessTools.Method(typeof(KnownTech), "Contains");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.DataboxAlwaysSpawn;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_1) &&
                    codes[i + 1].opcode.Equals(OpCodes.Call) && codes[i + 1].operand.Equals(methodKnownTechContains) &&
                    codes[i + 2].opcode.Equals(OpCodes.Brtrue))
                {
                    injected = true;
                    codes.RemoveRange(i, 3);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply DataboxSpawner_Start_patch.");
            return codes.AsEnumerable();
        }
    }
}
