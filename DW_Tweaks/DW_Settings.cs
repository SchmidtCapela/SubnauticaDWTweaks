using Oculus.Newtonsoft.Json;

namespace DW_Tweaks
{
    [JsonObject(MemberSerialization.OptOut)]
    public class DW_Tweaks_Settings
    {

        public bool HarmonyDebugging = false;                    // Enables the output of harmony.log.txt to the desktop

        public int InventoryWidth = 6;                           // Width of the player inventory, must be no more than 8
        public int InventoryHeight = 8;                          // Height of the player inventory, must be no more than 8

        public int SeamothInventoryWidth = 4;                    // Width of the seamoth cargo compartment, must be no more than 8
        public int SeamothInventoryHeight = 4;                   // Height of the seamoth cargo compartment, must be no more than 8

        public int BioReactorWidth = 4;                          // Width of the BioReactor storage, must be no more than 8
        public int BioReactorHeight = 4;                         // Height of the BioReactor storage, must be no more than 8

        public int ExosuitStorageWidth = 6;                      // Width of the PRAM storage, must be no more than 8

        public int ContainerOverstuff = 1;                       // Containers with only one kind of 1x1 item get their capacity multiplied by this

        public float FireExtinguisherRegen = 0f;                 // Amount of fuel the fire extinguisher regens per second when held

        public float SeamothDepthMod3 = 700f;                    // Extra depth that the MK3 mod allows for the Seamoth, added to its natural 200m max depth
        public float SeamothHandlingFix = 0f;                    // Fixes the seamoth handling and multiplies the speed by this value; 0 disables, 1.5766 is the old exploit speed

        public float CyclopsSpeedMult = 1f;                      // Myltiplies the Cyclops' speed by this value
        public float CyclopsTurningMult = 1f;                    // Myltiplies the Cyclops' turning by this value

        public float RenewablePowerPushExcess = 0f;              // Push this percentage of excess power from renewables into the base's infrastructure
        public float NonrenewablePowerPushExcess = 0f;           // Push this percentage of excess power from non-renewables into the base's infrastructure

        // Adjustments for the speed penalties for specific equipment
        public float speedTank = 0.425f;                         // Tank
        public float speedDoubleTank = 0.5f;                     // High Capacity Tank
        public float speedPlasteelTank = 0.10625f;               // Lightweight Tank
        public float speedHighCapacityTank = 0.6375f;            // Ultra High Capacity Tank
        public float speedHighCapacityTankInv = 1.275f;          // Ultra High Capacity Tank in inventory
        public float speedReinforcedDiveSuit = 1f;               // Reinforced Dive Suit, body only
        public float speedFins = 1.5f;                           // Fins
        public float speedUltraGlideFins = 2.5f;                 // Ultraglide Fins
        public float speedSwimChargeFins = 0f;                   // Swim Charge Fins
        public float speedHeldTool = 1f;                         // Speed bonus when not holding a tool

        public bool VehicleHUDExtraPrecision = false;            // Adds a decimal to Energy and Temperature views, and holding LeftControl and LeftShift provide alternative readouts
        public bool BypassBuildRestrictions = false;             // Holding LeftControl while building ignores certain restrictions
        public bool ConsoleDoesntDisableAchievements = false;    // Using the console doesn't disable achievements for the session. Active cheats still disable them.
        public bool FoodOvereatAlternateRule = false;            // Overeating is changed to avoid waste when eating with food above 99.
        public bool ScaleWithDayNightSpeed = false;              // daynightspeed extended to food, water, scanner.
        public bool HardcoreEnableSave = false;                  // Enables the save button when playing hardcore
        public bool PickupFullContainers = false;                // Enables picking up portable containers when they are full
        public bool UnlockSteamInventoryItems = false;           // Unlocks in-game items that usually require items in the player's Steam inventory
        public bool ContainerQuantityTooltip = false;            // Adds a tooltip saying how many items the container currently has, and the type if it only has one type or item
        public bool FragmentTrackAll = false;                    // Also track fragments you have already scanned
        public bool DrillableAlwaysDrop = false;                 // Always drop resources every time a large deposit breaks
        public bool DrillableDropMax = false;                    // Whenever large deposits break, drops the max amount of resources
        public bool SeaTreaderRandomChunks = false;              // Randomize the content of chunks unearthed by the Sea Treaders
        public bool DataboxAlwaysSpawn = false;                  // Spawn databoxes even if the player already knows the tech it gives

        public int Vector3StringPrecision = 1;                   // How many decimal places to show when printing Vector3 values, as seen in the F1 screen

        private static readonly DW_Tweaks_Settings instance = new DW_Tweaks_Settings();

        static DW_Tweaks_Settings()
        {
        }

        private DW_Tweaks_Settings()
        {
        }

        public static DW_Tweaks_Settings Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
