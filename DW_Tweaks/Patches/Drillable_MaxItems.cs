using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch]
    class Drillable_SpawnLootAsync_patch
    {
        public static readonly object fieldMinResourcesToSpawn = AccessTools.Field(typeof(Drillable), nameof(Drillable.minResourcesToSpawn));
        public static readonly object fieldMaxResourcesToSpawn = AccessTools.Field(typeof(Drillable), nameof(Drillable.maxResourcesToSpawn));

        // Need a different way to specify the target method due to it being a coroutine
        static MethodBase TargetMethod()//The target method is found using the custom logic defined here
        {
            var predicateClass = typeof(Drillable).GetNestedTypes(AccessTools.all)
                .FirstOrDefault(t => t.FullName.Contains("SpawnLootAsync"));//<SpawnLootAsync>d__45 is the hidden object's name, the number at the end of the name may vary. View the IL code to find out the name
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext")); //Look for the method MoveNext inside the hidden iterator object
        }

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.DrillableDropMax;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldloc_1) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldMinResourcesToSpawn))
                {
                    injected = true;
                    codes[i + 1].operand = fieldMaxResourcesToSpawn;
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Drillable_SpawnLootAsync_patch.");
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(Drillable), nameof(Drillable.DestroySelf))]
    class Drillable_DestroySelf_Patch  // Copied from Large Deposits Fix
    {
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.FixLargeDeposit;
        }

        static bool Prefix(Drillable __instance)
        {
            __instance.SendMessage("OnBreakResource", null, SendMessageOptions.DontRequireReceiver);
            return true;
        }
    }
}
