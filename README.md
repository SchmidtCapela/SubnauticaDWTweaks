## Requires the use of [QModManager](https://www.nexusmods.com/subnautica/mods/16/) found on Nexus Mods
### Based on [SNHardcorePlus](https://github.com/Qwiso/SNHardcorePlus), by Qwiso

### Installation
After installing QMods, extract the contents of [DW_Tweaks.zip](https://github.com/SchmidtCapela/SubnauticaDWTweaks/releases/download/1.1.0/DW_Tweaks.zip) file to your `\QMods` folder

### config.json
Should be located in `QMods\DW_Tweaks` folder. Every tweak in this mod can be customized, or at least toggled on or off, using this file.
If the file doesn't exist, a new one will be created with all tweaks disabled.
The description of each tweak, as well as the value that disables it, follows:

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
ContainerOverstuff | 1 | When a container only has a single type of 1x1 item its capacity is multiplied by this value.
FireExtinguisherRegen | 0.0 | Amount of fuel the fire extinguisher regens per second when held.
SeamothDepthMod3 | 700.0 | Extra depth that the MK3 mod allows for the Seamoth, added to its natural 200m max depth.
SeamothHandlingFix | 0.0 | If the value is above 0, corrects the Seamoth diagonals exploit (where going in a diagonal is faster than going forward), allow going at less than max speed when using an analog stick, and multiplies the speed by the given value. Setting it to 1 gives you the game's intended speed without the exploit, 1.5766 gives you the exploit speed.
CyclopsSpeedMult | 1.0 | How faster the Cyclops go.
CyclopsTurningMult | 1.0 | How faster the Cyclops turn.
VehicleHUDExtraPrecision | false | Adds a decimal to Energy and Temperature views, and holding LeftControl and LeftShift provide alternative readouts.
BypassBuildRestrictions | false | Holding LeftControl while building ignores certain restrictions: foundations can be built above other pieces, tubes can be built above each other, and the test to see if the player is inside or outside a base or the Cyclops is skipped. Allows building solar panels and thermal plants on the Cyclops. 
ConsoleDoesntDisableAchievements | false | Using the console doesn't disable achievements for the session. Active cheats still disable them.
FoodOvereatAlternateRule | false | The usual rule for overeating is that if you were already at food 99 or higher anything you ate would be wasted; with this enabled food you eat can still be added to your hunger bar, with the limit being 99 + the food value of whatever you ate.
HardcoreEnableSave | false | Enables the save button when playing hardcore. Because surviving crashes shouldn't be a required gameplay skill.
PickupFullContainers | false | Enables picking up portable containers even if they are full.
UnlockSteamInventoryItems | false | Unlocks in-game items that usually require items in the player's Steam inventory. Warning, it gets saved to the save files.
ContainerQuantityTooltip | false | Adds a tooltip saying how many items the container currently has, and the type if it only has one type or item.
FragmentTrackAll | false | Also track fragments you have already scanned.
DrillableAlwaysDrop | false | Always drop resources every time a large deposit breaks.
DrillableDropMax | false | Whenever large deposits break, drops the max amount of resources.
SeaTreaderRandomChunks | false | Randomize the content of chunks unearthed by the Sea Treaders, which can now give ten different kinds of resources.
RenewablePowerPushExcess | false | Push excess power from solar and thermal into the base's or cyclops' power relay, helping recharge other generators that aren't full, including the cyclops' batteries. Don't work across power transmitters, but it does work between generators built on the same foundation.
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
	"ContainerOverstuff": 1,
	"FireExtinguisherRegen": 0.0,
	"SeamothDepthMod3": 700.0,
	"SeamothHandlingFix": 0.0,
	"CyclopsSpeedMult": 1.0,
	"CyclopsTurningMult": 1.0,
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
	"RenewablePowerPushExcess": false,
	"Vector3StringPrecision": 1
}
```
