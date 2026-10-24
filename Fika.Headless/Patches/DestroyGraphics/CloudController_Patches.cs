using EFT.Rendering.Clouds;
using EFT.Weather;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class CloudController_OnEnable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CloudController).GetMethod(nameof(CloudController.OnEnable));
    }

    [PatchPrefix]
    public static bool Prefix(CloudController __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}

public class CloudController_UpdateAmbient_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WeatherController).GetMethod(nameof(WeatherController.ControlClouds));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
