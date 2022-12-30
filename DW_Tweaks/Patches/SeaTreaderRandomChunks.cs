using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using static HandReticle;
using System.Collections;
using UnityEngine.AddressableAssets;
using UWE;
using static iTween;
using static Oculus.Platform.Models.Room;
using static uGUI_OptionsPanel;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SeaTreaderSounds))]
    [HarmonyPatch("SpawnChunks")]
    class SeaTreaderSounds_SpawnChunks_patch
    {
        public static List<BreakableResource.RandomPrefab> prefabs = null;
        public static BreakableResource.RandomPrefab defPrefab = null;
        public static bool spawnDataInitialized = false;

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.SeaTreaderRandomChunks;
        }

        public static void Prefix(SeaTreaderSounds __instance, ref List<BreakableResource.RandomPrefab> __state)
        {
            if (__instance.stepChunkPrefab != null && prefabs != null && defPrefab != null)  // Change the spawn list for the seatreader's chunk
            {
                var breakableResource = __instance.stepChunkPrefab.GetComponent<BreakableResource>();
                __state = breakableResource.prefabList;
                __state.Add(new BreakableResource.RandomPrefab() { prefabTechType = breakableResource.defaultPrefabTechType, prefabReference = breakableResource.defaultPrefabReference, chance = 0 });
                breakableResource.prefabList = prefabs;
                breakableResource.defaultPrefabReference = defPrefab.prefabReference;
                breakableResource.defaultPrefabTechType = defPrefab.prefabTechType;
            }
            else __state = null;
            return;
        }

        public static void Postfix(SeaTreaderSounds __instance, List<BreakableResource.RandomPrefab> __state)
        {
            if (__state != null)
            {
                var breakableResource = __instance.stepChunkPrefab.GetComponent<BreakableResource>();
                breakableResource.defaultPrefabReference = __state[__state.Count - 1].prefabReference;
                breakableResource.defaultPrefabTechType = __state[__state.Count - 1].prefabTechType;
                __state.RemoveAt(__state.Count - 1);
                breakableResource.prefabList = __state;
            }
            return;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    public static class Player_Awake_Patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return DW_Tweaks_Settings.Instance.SeaTreaderRandomChunks;
        }

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

        [HarmonyPostfix]
        public static void Postfix(Player __instance)
        {
            CoroutineHost.StartCoroutine(GetPrefabForList());
        }

        private static IEnumerator GetPrefabForList()
        {
            if (!SeaTreaderSounds_SpawnChunks_patch.spawnDataInitialized)  // Needs to add the extra materials to the entropy lists, otherwise they can't come from chunks
            {
                PlayerEntropy component = Player.main.gameObject.GetComponent<PlayerEntropy>();
                if (component != null)
                {
                    SeaTreaderSounds_SpawnChunks_patch.spawnDataInitialized = true;
                    float totalChance = techList.Sum(x => x.chance);
                    // Load the prefabs later to avoid conflicts with other mods
                    // CoroutineHost.StartCoroutine(GetPrefabForList(techList.Last().tech, 0));
                    CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(techList.Last().tech, false);
                    yield return task;
                    GameObject gameObject = task.GetResult();
                    if (gameObject != null && PrefabDatabase.TryGetPrefabFilename(gameObject.GetComponent<PrefabIdentifier>().ClassId, out string filename))
                        SeaTreaderSounds_SpawnChunks_patch.defPrefab = new BreakableResource.RandomPrefab() { prefabTechType = techList.Last().tech, prefabReference = new AssetReferenceGameObject(filename), chance = 0 };
                    SeaTreaderSounds_SpawnChunks_patch.prefabs = new List<BreakableResource.RandomPrefab>();
                    for (int i = 0; i < techList.Length; i++)
                    {
                        // Adds the prefab and calculated chance to the list
                        if (i < techList.Length - 1)
                        {
                            // CoroutineHost.StartCoroutine(GetPrefabForList(techList[i].tech, techList[i].chance / totalChance));
                            task = CraftData.GetPrefabForTechTypeAsync(techList[i].tech, false);
                            yield return task;
                            gameObject = task.GetResult();
                            if (gameObject != null && PrefabDatabase.TryGetPrefabFilename(gameObject.GetComponent<PrefabIdentifier>().ClassId, out filename))
                                SeaTreaderSounds_SpawnChunks_patch.prefabs.Add(new BreakableResource.RandomPrefab() { prefabTechType = techList[i].tech, prefabReference = new AssetReferenceGameObject(filename), chance = techList[i].chance / totalChance });
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
            yield break;
        }
    }
}
