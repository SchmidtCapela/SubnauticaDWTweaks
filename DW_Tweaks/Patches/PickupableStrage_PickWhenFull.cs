using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(PickupableStorage))]
    [HarmonyPatch("OnHandHover")]
    class PickupableStorage_OnHandHover_patch
    {
        public static readonly object methodOnHandHover = AccessTools.Method(typeof(Pickupable), "OnHandHover");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.PickupFullContainers;
        }

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool injected = false;
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldarg_1) &&
                    codes[i + 3].opcode.Equals(OpCodes.Callvirt) && codes[i + 3].operand.Equals(methodOnHandHover) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ret)
                    )
                {
                    injected = true;
                    codes.RemoveRange(0, i);
                    codes.RemoveRange(4, codes.Count - 5);
                    break;
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply PickupableStorage_OnHandHover_patch.");
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(PickupableStorage))]
    [HarmonyPatch("OnHandClick")]
    class PickupableStorage_OnHandClick_patch
    {
        public static readonly object methodOnHandClick = AccessTools.Method(typeof(Pickupable), "OnHandClick");

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool injected = false;
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!injected &&
                    codes[i].opcode.Equals(OpCodes.Ldarg_0) &&
                    codes[i + 1].opcode.Equals(OpCodes.Ldfld) &&
                    codes[i + 2].opcode.Equals(OpCodes.Ldarg_1) &&
                    codes[i + 3].opcode.Equals(OpCodes.Callvirt) && codes[i + 3].operand.Equals(methodOnHandClick) &&
                    codes[i + 4].opcode.Equals(OpCodes.Ret)
                    )
                {
                    injected = true;
                    codes.RemoveRange(0, i);
                    codes.RemoveRange(4, codes.Count - 5);
                }
            }
            if (!injected) Console.WriteLine("DW_Tweaks ERR: Failed to apply PickupableStorage_OnHandClick_patch.");
            return codes.AsEnumerable();
        }
    }
}
