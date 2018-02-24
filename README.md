## Requires the use of [QModManager](https://www.nexusmods.com/subnautica/mods/16/) found on Nexus Mods

### Installation
After installing QMods, extract the contents of [DW_Tweaks.zip](https://github.com/SchmidtCapela/SubnauticaDWTweaks/releases/download/1.0.0/DW_Tweaks.zip) file to your `\QMods` folder

### config.json
Should be located in `QMods\DW_Tweaks` folder. Every tweak in this mod can be customized, or at least toggled on or off, in this file.
If the file doesn't exist, a new one will be created with all tweaks disabled.

Name | Value | Description
:--- | :-----: | :---
HarmonyDebugging | false | Enables the output of harmony.log.txt to the desktop; for debugging only.
InventoryWidth | 6 | Width of the player inventory, valid values are between 4 and 8, inclusive.
InventoryHeight | 8 | Height of the player inventory, valid values are between 4 and 8, inclusive.
SeamothInventoryWidth | 4 | Width of the seamoth cargo compartment, valid values are between 4 and 8, inclusive.
SeamothInventoryHeight | 4 | Height of the seamoth cargo compartment, valid values are between 4 and 8, inclusive.
BioReactorWidth | 4 | Width of the bioreactor storage, valid values are between 4 and 8, inclusive.
BioReactorHeight | 4 | Height of the bioreactor storage, valid values are between 4 and 8, inclusive.
ExosuitStorageWidth | 6 | Width of the PRAM storage, valid values are between 4 and 8, inclusive.
FireExtinguisherRegen | 0.0 | Amount of fuel the fire extinguisher regens per second when held.
SeamothDepthMod3 | 700.0 | Extra depth that the MK3 mod allows for the Seamoth, added to its natural 200m max depth.
VehicleHUDExtraPrecision | false | Adds a decimal to Energy and Temperature views, and holding LeftControl and LeftShift provide alternative readouts.
BypassBuildRestrictions | false | Holding LeftControl while building ignores certain restrictions.
ConsoleDoesntDisableAchievements | false | Using the console doesn't disable achievements for the session. Active cheats still disable them.
FoodOvereatAlternateRule | false | Overeating is changed to avoid waste when eating with food above 99.
HardcoreEnableSave | false | Enables the save button when playing hardcore.
PickupFullContainers | false | Enables picking up portable containers when they are full.
UnlockSteamInventoryItems | false | Unlocks in-game items that usually require items in the player's Steam inventory.
ContainerQuantityTooltip | false | Adds a tooltip saying how many items the container currently has, and the type if it only has one type or item.
FragmentTrackAll | false | Also track fragments you have already scanned.
DrillableAlwaysDrop | false | Always drop resources every time a large deposit breaks.
DrillableDropMax | false | Whenever large deposits break, drops the max amount of resources.
SeaTreaderRandomChunks | false | Randomize the content of chunks unearthed by the Sea Treaders.
Vector3StringPrecision | 1 | How many decimal places to show when printing Vector3 values, as seen in the F1 screen.


```
{
	"HarmonyDebugging": false,
	"InventoryWidth": 6,
	"InventoryHeight": 8,
	"SeamothInventoryWidth": 4,
	"SeamothInventoryHeight": 4,
	"BioReactorWidth": 4,
	"BioReactorHeight": 4,
	"ExosuitStorageWidth": 6,
	"FireExtinguisherRegen": 0.0,
	"SeamothDepthMod3": 700.0,
	"VehicleHUDExtraPrecision": false,
	"BypassBuildRestrictions": false,
	"ConsoleDoesntDisableAchievements": false,
	"FoodOvereatAlternateRule": false,
	"HardcoreEnableSave": false,
	"PickupFullContainers": false,
	"UnlockSteamInventoryItems": false,
	"ContainerQuantityTooltip": false,
	"FragmentTrackAll": false,
	"DrillableAlwaysDrop": false,
	"DrillableDropMax": false,
	"SeaTreaderRandomChunks": false,
	"Vector3StringPrecision": 1
}
```
