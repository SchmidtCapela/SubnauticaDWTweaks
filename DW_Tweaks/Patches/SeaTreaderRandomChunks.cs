using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SeaTreaderSounds))]
    [HarmonyPatch("SpawnChunks")]
    class SeaTreaderSounds_SpawnChunks_patch
    {
        public static List<BreakableResource.RandomPrefab> prefabs = new List<BreakableResource.RandomPrefab> {
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Diamond), chance = (float)0.0500},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.AluminumOxide), chance = (float)0.0526 },
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Lead), chance = (float)0.0556},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Lithium), chance = (float)0.0588},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Copper), chance = (float)0.1250},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Silver), chance = (float)0.1429},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Gold), chance = (float)0.16667},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Magnetite), chance = (float)0.2},
                  new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(TechType.Titanium), chance = (float)0.5},
                };
        public static GameObject defPrefab = CraftData.GetPrefabForTechType(TechType.Quartz);
        public static bool entropyListPatched = false;

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.SeaTreaderRandomChunks;
        }

        public static void Prefix(SeaTreaderSounds __instance, ref List<BreakableResource.RandomPrefab> __state)
        {
            if (!entropyListPatched)  // Needs to add the extra materials to the entropy lists, otherwise they can't come from chunks
            {
                TechType[] techList = { TechType.Diamond, TechType.AluminumOxide, TechType.Lead, TechType.Lithium, TechType.Copper, TechType.Silver, TechType.Gold, TechType.Magnetite, TechType.Titanium, TechType.Quartz };
                PlayerEntropy component = Player.main.gameObject.GetComponent<PlayerEntropy>();
                if (component != null)
                {
                    entropyListPatched = true;
                    for (int i = 0; i < techList.Length; i++)
                    {
                        bool exists = false;
                        for (int j = 0; j < component.randomizers.Count; j++)
                        {
                            if (component.randomizers[j].techType == techList[i])
                            {
                                exists = true;
                            }
                        }
                        if (!exists) component.randomizers.Add(new PlayerEntropy.TechEntropy() { techType = techList[i], entropy = new FairRandomizer() { entropy = 0 } });
                    }

                }
            }
            if (__instance.stepChunkPrefab != null)  // Can change the 
            {
                var breakableResource = __instance.stepChunkPrefab.GetComponent<BreakableResource>();
                __state = breakableResource.prefabList;
                __state.Add(new BreakableResource.RandomPrefab() { prefab = breakableResource.defaultPrefab });
                breakableResource.prefabList = prefabs;
                breakableResource.defaultPrefab = defPrefab;
            }
            else __state = null;
            return;
        }

        public static void Postfix(SeaTreaderSounds __instance, List<BreakableResource.RandomPrefab> __state)
        {
            if (__state != null)
            {
                var breakableResource = __instance.stepChunkPrefab.GetComponent<BreakableResource>();
                breakableResource.defaultPrefab = __state[__state.Count - 1].prefab;
                __state.RemoveAt(__state.Count - 1);
                breakableResource.prefabList = __state;
            }
            return;
        }
    }
}
