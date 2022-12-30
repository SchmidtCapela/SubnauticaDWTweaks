using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(Story.StoryGoalManager), nameof(Story.StoryGoalManager.ExecutePendingRadioMessage))]
    class StoryGoalManager_ExecutePendingRadioMessage_patch  // Remove the countdown by making that specific radio message not start its associated story goal;
                                                             // the countdown can be started with the console command "sunbeamcountdownstart"
    {
        public static readonly object funcStringEquals = AccessTools.Method(typeof(String), nameof(String.Equals), new Type[] { typeof(string), typeof(string), typeof(StringComparison) });
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 4; i++)
            {
                if (!injected &&
                    Utils.isCode(codes[i    ], OpCodes.Brfalse) && // Jump to skip OnGoalComplete; we will use the label
                    Utils.isCode(codes[i + 1], OpCodes.Ldarg_0) && // Reference to StoryGoalManager; we won't use it
                    Utils.isCode(codes[i + 2], OpCodes.Ldstr, "OnPlay") &&
                    Utils.isCode(codes[i + 3], OpCodes.Ldloc_0) && // entryData
                    Utils.isCode(codes[i + 4], OpCodes.Ldfld))     // field "key"; we will use this.
                {
                    injected = true;
                    codes.InsertRange(i + 1, new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Ldfld, codes[i + 4].operand),
                        new CodeInstruction(OpCodes.Ldstr, "RadioSunbeam4"),
                        new CodeInstruction(OpCodes.Ldc_I4_5),
                        new CodeInstruction(OpCodes.Call, funcStringEquals),
                        new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand),
                    });
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply StoryGoalManager_ExecutePendingRadioMessage_patch.");
            return codes.AsEnumerable();
        }
    }

    // Patch to disable the countdown if the value ever goes negative
    [HarmonyPatch(typeof(uGUI_SunbeamCountdown), nameof(uGUI_SunbeamCountdown.UpdateInterface))]
    class uGUI_SunbeamCountdown_UpdateInterface_Patch
    {
        public static bool Prefix()
        {
            if (StoryGoalCustomEventHandler.main && (StoryGoalCustomEventHandler.main.endTime < DayNightCycle.main.timePassedAsFloat))
            {
                StoryGoalCustomEventHandler.main.countdownActive = false;
            }
            return true;
        }
    }
}
