## Requires the use of [QModManager](https://www.nexusmods.com/subnautica/mods/16/) found on Nexus Mods
### Based on [SNHardcorePlus](https://github.com/Qwiso/SNHardcorePlus), by Qwiso

### Installation
After installing QMods, extract the contents of [DW_Tweaks.zip](https://github.com/SchmidtCapela/SubnauticaDWTweaks/releases/download/1.3.0/DW_Tweaks.zip) file to your `\QMods` folder.

### config.json
Should be located in `QMods\DW_Tweaks` folder. Every tweak in this mod can be customized, or at least toggled on or off, using this file.

To toggle a tweak off, either set it to the default value (found in the table below) or delete it.

The mod ships with my personal file, which has all tweaks enabled except for the inventory size ones and the Harmony debugging. If you want to change inventory sizes I suggest using the [CustomizedStorage](https://www.nexusmods.com/subnautica/mods/35) mod instead, as it allows customizing all inventories my mod tweaks and more.

If the config.json file doesn't exist, a new one will be created with all tweaks disabled.

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
CyclopsSpeedMult | 1.0 | How faster the Cyclops go; 1.5 is 50% faster.
CyclopsTurningMult | 1.0 | How faster the Cyclops turn; 1.5 is 50% faster.
RenewablePowerPushExcess | 0.0 | Push this portion of the excess power from solar and thermal into the base's or cyclops' power relay, helping recharge other generators that aren't full, including the cyclops' batteries. 1.0 pushes all extra power.
speedTank | 0.425 | How much the air tank slows down the player.
speedDoubleTank | 0.5 | How much the high capacity tank slows the player.
speedPlasteelTank | 0.10625 | How much the lightweight tank slows the player.
speedHighCapacityTank | 0.6375 | How much the ultra high capacity tank slows the player.
speedHighCapacityTankInv | 1.275 | How much the ultra high capacity tank slows the player while stored in the inventory.
speedReinforcedDiveSuit | 1 | How much the reinforced diving suit slows the player.
speedFins | 1.5 | How much speed the common fins add.
speedUltraGlideFins | 2.5 | How much speed the ultraglide fins add.
speedSwimChargeFins | 0.0 | How much speed the swim charge fins add.
speedHeldTool | 1.0 | How much speed the player gains by not having a tool on his hands. Note, this is a bonus.
VehicleHUDExtraPrecision | false | Adds a decimal to Energy and Temperature views, and holding LeftControl and LeftShift provide alternative readouts. Also enables displaying energy values while inside the Cyclops as if it was a base.
BypassBuildRestrictions | false | Holding LeftControl while building ignores certain restrictions: foundations can be built above other pieces, tubes can be built above each other, the test to see if the player is inside or outside a base or the Cyclops is skipped, and interior pieces built on the surface of a base or the Cyclops count as being inside it. Among other things allows building solar panels and thermal plants on the Cyclops.
ConsoleDoesntDisableAchievements | false | Using the console doesn't disable achievements for the session. Active cheats still disable them.
FoodOvereatAlternateRule | false | The usual rule for overeating is that if you were already at food 99 or higher anything you ate would be wasted; with this enabled food you eat can still be added to your hunger bar, with the limit being 99 + the food value of whatever you ate.
ScaleWithDayNightSpeed | false | Extending scaling with the daynightspeed command to food and water consumption, as well as scan room energy usage.
HardcoreEnableSave | false | Enables the save button when playing hardcore. Because surviving crashes shouldn't be a required gameplay skill.
PickupFullContainers | false | Enables picking up portable containers even if they are full.
UnlockSteamInventoryItems | false | Unlocks in-game items that usually require items in the player's Steam inventory. Warning, it gets saved to the save files.
ContainerQuantityTooltip | false | Adds a tooltip saying how many items the container currently has, and the type if it only has one type or item.
FragmentTrackAll | false | Also track fragments you have already scanned.
DrillableAlwaysDrop | false | Always drop resources every time a large deposit breaks.
DrillableDropMax | false | Whenever large deposits break, drops the max amount of resources.
SeaTreaderRandomChunks | false | Randomize the content of chunks unearthed by the Sea Treaders, which can now give ten different kinds of resources.
DataboxAlwaysSpawn | false | By default the game doesn't spawn databoxes and fragments in wrecks for which the player already knows the technology it gives; with this tweak databoxes and fragments are always spawned.
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
	"RenewablePowerPushExcess": 0.0,
	"speedTank": 0.425,
	"speedDoubleTank": 0.5,
	"speedPlasteelTank": 0.10625,
	"speedHighCapacityTank": 0.6375,
	"speedHighCapacityTankInv": 1.275,
	"speedReinforcedDiveSuit": 1,
	"speedFins": 1.5,
	"speedUltraGlideFins": 2.5,
	"speedSwimChargeFins": 0.0,
	"speedHeldTool": 1.0,
	"VehicleHUDExtraPrecision": false,
	"BypassBuildRestrictions": false,
	"ConsoleDoesntDisableAchievements": false,
	"FoodOvereatAlternateRule": false,
	"ScaleWithDayNightSpeed": false,
	"HardcoreEnableSave": false,
	"PickupFullContainers": false,
	"UnlockSteamInventoryItems": false,
	"ContainerQuantityTooltip": false,
	"FragmentTrackAll": false,
	"DrillableAlwaysDrop": false,
	"DrillableDropMax": false,
	"SeaTreaderRandomChunks": false,
	"DataboxAlwaysSpawn": false,
	"Vector3StringPrecision": 1
}
```
