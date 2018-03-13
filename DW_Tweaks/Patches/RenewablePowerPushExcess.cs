using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace DW_Tweaks.Patches
{
    [HarmonyPatch(typeof(SolarPanel))]
    [HarmonyPatch("Update")]
    class SolarPanel_Update_patch
    {
        public static readonly MethodInfo funcGetRechargeScalar = AccessTools.Method(typeof(SolarPanel), "GetRechargeScalar");

        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.RenewablePowerPushExcess != 0);
        }

        public static bool Prefix(SolarPanel __instance)
        {
            Constructable component = __instance.gameObject.GetComponent<Constructable>();
            if (component.constructed)
            {
                float producedPower = (float)funcGetRechargeScalar.Invoke(__instance, new object[] { }) * DayNightCycle.main.deltaTime * 0.25f * 5f;
                float excessPower = Mathf.Max(0f, producedPower - (__instance.powerSource.maxPower - __instance.powerSource.power)) * DW_Tweaks_Settings.Instance.RenewablePowerPushExcess;
                __instance.powerSource.power = Mathf.Clamp(__instance.powerSource.power + producedPower, 0f, __instance.powerSource.maxPower);
                if (excessPower > 0)
                {
                    float transferredPower = 0;
                    if (__instance.powerSource.connectedRelay != null)
                    {
                        if (!__instance.powerSource.connectedRelay.AddEnergy(excessPower, out transferredPower))
                            __instance.GetComponent<PowerRelay>().GetEndpoint().AddEnergy(excessPower - transferredPower, out transferredPower);
                    }
                    else __instance.GetComponent<PowerRelay>().GetEndpoint().AddEnergy(excessPower, out transferredPower);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ThermalPlant))]
    [HarmonyPatch("AddPower")]
    class ThermalPlant_AddPower_patch
    {
        // Test to see if using default values, skip patching if true
        public static bool Prepare()
        {
            return (DW_Tweaks_Settings.Instance.RenewablePowerPushExcess != 0);
        }

        public static bool Prefix(ThermalPlant __instance)
        {
            if (__instance.constructable.constructed && __instance.temperature > 25f)
            {
                float dayNightSpeed = 2f * DayNightCycle.main.dayNightSpeed;
                float producedPower = 1.6500001f * dayNightSpeed * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, __instance.temperature));
                float storedPower = 0f;
                if (!__instance.powerSource.AddEnergy(producedPower, out storedPower))
                {
                    float excessPower = (producedPower - storedPower) * DW_Tweaks_Settings.Instance.RenewablePowerPushExcess;
                    float transferredPower = 0;
                    if (__instance.powerSource.connectedRelay != null)
                    {
                        if (!__instance.powerSource.connectedRelay.AddEnergy(excessPower, out transferredPower))
                            __instance.GetComponent<PowerRelay>().GetEndpoint().AddEnergy(excessPower - transferredPower, out transferredPower);
                    }
                    else __instance.GetComponent<PowerRelay>().GetEndpoint().AddEnergy(excessPower, out transferredPower);

                }
            }
            return false;
        }
    }
}
