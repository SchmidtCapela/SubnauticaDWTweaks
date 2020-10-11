using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(DataboxSpawner), nameof(DataboxSpawner.Start))]
    class DataboxSpawner_Start_patch
    {
        public static readonly object methodKnownTechContains = AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Contains));

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
                    codes[i].opcode.Equals(OpCodes.Ldloc_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 2].opcode.Equals(OpCodes.Call) && codes[i + 2].operand.Equals(methodKnownTechContains) &&
                    codes[i + 3].opcode.Equals(OpCodes.Brtrue))
                {
                    injected = true;
                    codes.RemoveRange(i, 4);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply DataboxSpawner_Start_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(CSVEntitySpawner), nameof(CSVEntitySpawner.GetPrefabForSlot))]
    class CSVEntitySpawner_GetPrefabForSlot_patch
    {
        public static readonly object methodKnownTechContains = AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Contains));

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.DataboxAlwaysSpawn;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_2) &&
                    codes[i + 1].opcode.Equals(OpCodes.Brfalse))
                {
                    injected = true;
                    codes.RemoveAt(i);
                    codes[i].opcode = OpCodes.Br;
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply CSVEntitySpawner_GetPrefabForSlot_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ResourceTracker), nameof(ResourceTracker.Start))]
    internal class ResourceTracker_Patcher  // Copied from Databox Scanner Fix
    {
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.FixDataboxScan;
        }

        [HarmonyPostfix]
        internal static void PostFix(ref ResourceTracker __instance)
        {
            bool isDataBox = __instance.overrideTechType == TechType.Databox ||
                __instance.techType == TechType.Databox;

            if (!isDataBox)
                return; // Not a data box, early exit

            var blueprint = __instance.GetComponentInParent<BlueprintHandTarget>();

            if (blueprint == null)
                return; // safety check, but shouldn't happen

            if (!blueprint.used)
                return; // blueprint still unused

            __instance.OnBreakResource(); // call this to invoke the "Unregister" method
        }
    }

    [HarmonyPatch(typeof(BlueprintHandTarget), nameof(BlueprintHandTarget.UnlockBlueprint))]
    internal class BlueprintHandTarget_Patcher  // Copied from Databox Scanner Fix
    {
        [HarmonyPrefix]
        internal static bool PreFix(ref BlueprintHandTarget __instance)
        {
            __instance.SendMessage("OnBreakResource", null, SendMessageOptions.DontRequireReceiver);

            return true;
        }
    }
}
