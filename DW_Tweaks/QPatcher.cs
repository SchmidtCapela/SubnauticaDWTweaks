using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace DW_Tweaks
{
    public static class Utils
    {
        public static bool isCode(CodeInstruction code, OpCode opcode)
        {
            if (opcode.Equals(OpCodes.Ldarg) || opcode.Equals(OpCodes.Ldarg_S))
            {
                return code.opcode.Equals(OpCodes.Ldarg_0) || code.opcode.Equals(OpCodes.Ldarg_1) || code.opcode.Equals(OpCodes.Ldarg_2) || code.opcode.Equals(OpCodes.Ldarg_3)
                    || code.opcode.Equals(OpCodes.Ldarg) || code.opcode.Equals(OpCodes.Ldarg_S);
            }
            if (opcode.Equals(OpCodes.Ldloc) || opcode.Equals(OpCodes.Ldloc_S))
            {
                return code.opcode.Equals(OpCodes.Ldloc_0) || code.opcode.Equals(OpCodes.Ldloc_1) || code.opcode.Equals(OpCodes.Ldloc_2) || code.opcode.Equals(OpCodes.Ldloc_3)
                    || code.opcode.Equals(OpCodes.Ldloc) || code.opcode.Equals(OpCodes.Ldloc_S);
            }
            if (opcode.Equals(OpCodes.Stloc) || opcode.Equals(OpCodes.Stloc_S))
            {
                return code.opcode.Equals(OpCodes.Stloc_0) || code.opcode.Equals(OpCodes.Stloc_1) || code.opcode.Equals(OpCodes.Stloc_2) || code.opcode.Equals(OpCodes.Stloc_3)
                    || code.opcode.Equals(OpCodes.Stloc) || code.opcode.Equals(OpCodes.Stloc_S);
            }
            return code.opcode.Equals(opcode);
        }
        public static bool isCode(CodeInstruction orig, OpCode opcode, object operand)
        {
            return orig.opcode.Equals(opcode) && orig.operand.Equals(operand);
        }
        public static object getLocalVar(CodeInstruction orig)
        {
            if (orig.opcode.Equals(OpCodes.Ldloc) || orig.opcode.Equals(OpCodes.Ldloc_S) || orig.opcode.Equals(OpCodes.Stloc) || orig.opcode.Equals(OpCodes.Stloc_S))
            { return orig.operand; }
            if (orig.opcode.Equals(OpCodes.Ldloc_0) || orig.opcode.Equals(OpCodes.Stloc_0))
            { return 0; }
            if (orig.opcode.Equals(OpCodes.Ldloc_1) || orig.opcode.Equals(OpCodes.Stloc_1))
            { return 1; }
            if (orig.opcode.Equals(OpCodes.Ldloc_2) || orig.opcode.Equals(OpCodes.Stloc_2))
            { return 2; }
            if (orig.opcode.Equals(OpCodes.Ldloc_3) || orig.opcode.Equals(OpCodes.Stloc_3))
            { return 3; }
            return null;
        }
    }
    [BepInPlugin("qwiso.dw_tweaks.mod", "DW Tweaks", "2.0.0")]
    public class MyPlugin : BaseUnityPlugin
    {
        // You may also use Awake(), Update(), LateUpdate() etc..
        // as BepInEx plugins are MonoBehaviours.
        private void Start()
        {
            manageSettingsFile();

            Harmony.DEBUG = DW_Tweaks_Settings.Instance.HarmonyDebugging;
            var harmony = new Harmony("qwiso.dw_tweaks.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        private static void manageSettingsFile()
        {
            string settingsPath = Path.Combine(Paths.ConfigPath, "DW_Tweaks.json");

            if (!File.Exists(settingsPath))
            {
                writeDefaultSettingsFile(settingsPath);
                return;
            }

            var userSettings = JsonConvert.DeserializeObject<DW_Tweaks_Settings>(File.ReadAllText(settingsPath));
            applyUserSettings(userSettings);
        }


        private static void applyUserSettings(DW_Tweaks_Settings userSettings)
        {
            var fields = typeof(DW_Tweaks_Settings).GetFields();

            foreach (var field in fields)
            {
                var userValue = field.GetValue(userSettings);
                field.SetValue(DW_Tweaks_Settings.Instance, userValue);
            }

            // Boundary checks
            if (DW_Tweaks_Settings.Instance.InventoryWidth < 4 || DW_Tweaks_Settings.Instance.InventoryWidth > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter InventoryWidth out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.InventoryWidth = 6;
            }
            if (DW_Tweaks_Settings.Instance.InventoryHeight < 4 || DW_Tweaks_Settings.Instance.InventoryHeight > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter InventoryHeight out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.InventoryHeight = 8;
            }
            if (DW_Tweaks_Settings.Instance.SeamothInventoryWidth < 4 || DW_Tweaks_Settings.Instance.SeamothInventoryWidth > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter InventoryWidth out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.SeamothInventoryWidth = 4;
            }
            if (DW_Tweaks_Settings.Instance.SeamothInventoryHeight < 4 || DW_Tweaks_Settings.Instance.SeamothInventoryHeight > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter InventoryHeight out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.SeamothInventoryHeight = 4;
            }
            if (DW_Tweaks_Settings.Instance.BioReactorWidth < 4 || DW_Tweaks_Settings.Instance.BioReactorWidth > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter BioReactorWidth out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.BioReactorWidth = 4;
            }
            if (DW_Tweaks_Settings.Instance.BioReactorHeight < 4 || DW_Tweaks_Settings.Instance.BioReactorHeight > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter BioReactorHeight out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.BioReactorHeight = 4;
            }
            if (DW_Tweaks_Settings.Instance.ExosuitStorageWidth < 4 || DW_Tweaks_Settings.Instance.ExosuitStorageWidth > 8)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter ExosuitStorageWidth out of bounds. Should be between 4 and 8.");
                DW_Tweaks_Settings.Instance.ExosuitStorageWidth = 6;
            }
            if (DW_Tweaks_Settings.Instance.ContainerOverstuff < 1)
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter ContainerOverstuff out of bounds. Should be 1 or more.");
                DW_Tweaks_Settings.Instance.ContainerOverstuff = 1;
            }
            if ((DW_Tweaks_Settings.Instance.RenewablePowerPushExcess > 1f) || (DW_Tweaks_Settings.Instance.RenewablePowerPushExcess < 0f))
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter RenewablePowerPushExcess out of bounds. Should be between 0 and 1.");
                DW_Tweaks_Settings.Instance.RenewablePowerPushExcess = 0f;
            }
            if ((DW_Tweaks_Settings.Instance.NonrenewablePowerPushExcess > 1f) || (DW_Tweaks_Settings.Instance.NonrenewablePowerPushExcess < 0f))
            {
                Console.WriteLine("DW_Tweaks ERR: Parameter NonrenewablePowerPushExcess out of bounds. Should be between 0 and 1.");
                DW_Tweaks_Settings.Instance.NonrenewablePowerPushExcess = 0f;
            }
        }

        private static void writeDefaultSettingsFile(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(DW_Tweaks_Settings.Instance));
        }
    }
}
