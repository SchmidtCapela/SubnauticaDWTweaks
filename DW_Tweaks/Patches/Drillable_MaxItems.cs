using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Drillable), nameof(Drillable.OnDrill))]
    class Drillable_OnDrill_patch
    {
        public static readonly object fieldChanceToSpawnResources = AccessTools.Field(typeof(Drillable), "kChanceToSpawnResources");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.DrillableAlwaysDrop;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldChanceToSpawnResources))
                {
                    injected = true;
                    codes.RemoveRange(i, 2);
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldc_R4, (float)10));  // needs to be more than 1
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Drillable_OnDrill_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Drillable), nameof(Drillable.SpawnLoot))]
    class Drillable_SpawnLoot_patch
    {
        public static readonly object fieldMinResourcesToSpawn = AccessTools.Field(typeof(Drillable), nameof(Drillable.minResourcesToSpawn));
        public static readonly object fieldMaxResourcesToSpawn = AccessTools.Field(typeof(Drillable), nameof(Drillable.maxResourcesToSpawn));

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
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) && codes[i + 1].operand.Equals(fieldMinResourcesToSpawn))
                {
                    injected = true;
                    codes[i + 1].operand = fieldMaxResourcesToSpawn;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply Drillable_SpawnLoot_patch.");
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
