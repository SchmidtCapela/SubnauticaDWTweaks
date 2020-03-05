using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using QModManager.API.ModLoading;

namespace DW_Tweaks
{
    [QModCore]
    public static class QPatch
    {
        [QModPatch]
        public static void Patch()
        {
            manageSettingsFile();
            
            HarmonyInstance harmony = HarmonyInstance.Create("qwiso.dw_tweaks.mod");
            HarmonyInstance.DEBUG = DW_Tweaks_Settings.Instance.HarmonyDebugging;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        private static void manageSettingsFile()
        {
            string modDirectory = Environment.CurrentDirectory + @"\QMods";
            string settingsPath = modDirectory + @"\DW_Tweaks\config.json";

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
