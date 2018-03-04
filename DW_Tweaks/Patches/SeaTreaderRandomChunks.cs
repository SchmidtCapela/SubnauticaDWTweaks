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
        public static List<BreakableResource.RandomPrefab> prefabs = null;
        public static GameObject defPrefab = null;
        public static bool spawnDataInitialized = false;

        private struct TechChance
        {
            public TechType tech;
            public float chance;
            public TechChance(TechType t, float c)
            {
                tech = t;
                chance = c;
            }
        }
        private static TechChance[] techList =
        {
            new TechChance(TechType.Diamond, 1f),
            new TechChance(TechType.AluminumOxide, 1f),
            new TechChance(TechType.Lead, 1f),
            new TechChance(TechType.Lithium, 1f),
            new TechChance(TechType.Copper, 2f),
            new TechChance(TechType.Silver, 2f),
            new TechChance(TechType.Gold, 2f),
            new TechChance(TechType.Magnetite, 2f),
            new TechChance(TechType.Titanium, 4f),
            new TechChance(TechType.Quartz, 4f),
        };

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.SeaTreaderRandomChunks;
        }

        public static void Prefix(SeaTreaderSounds __instance, ref List<BreakableResource.RandomPrefab> __state)
        {
            if (!spawnDataInitialized)  // Needs to add the extra materials to the entropy lists, otherwise they can't come from chunks
            {
                PlayerEntropy component = Player.main.gameObject.GetComponent<PlayerEntropy>();
                if (component != null)
                {
                    spawnDataInitialized = true;
                    float totalChance = techList.Sum(x => x.chance);
                    // Load the prefabs later to avoid conflicts with other mods
                    defPrefab = CraftData.GetPrefabForTechType(techList.Last().tech);
                    prefabs = new List<BreakableResource.RandomPrefab>();
                    for (int i = 0; i < techList.Length; i++)
                    {
                        // Adds the prefab and calculated chance to the list
                        if (i < techList.Length - 1)
                        {
                            prefabs.Add(new BreakableResource.RandomPrefab() { prefab = CraftData.GetPrefabForTechType(techList[i].tech), chance = techList[i].chance / totalChance });
                            totalChance -= techList[i].chance;
                        }
                        // Add the TechType to the entropy lists
                        bool exists = false;
                        for (int j = 0; j < component.randomizers.Count; j++)
                        {
                            if (component.randomizers[j].techType == techList[i].tech)
                            {
                                exists = true;
                            }
                        }
                        if (!exists) component.randomizers.Add(new PlayerEntropy.TechEntropy() { techType = techList[i].tech, entropy = new FairRandomizer() { entropy = 0 } });
                    }

                }
            }
            if (__instance.stepChunkPrefab != null)  // Change the spawn list for the seatreader's chunk
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
