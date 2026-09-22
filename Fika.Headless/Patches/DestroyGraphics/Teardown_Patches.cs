using EFT.Passcodes;
using EFT.Visual;
using Il2CppSystems.Effects;
using Koenigz.PerfectCulling.EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class ScopeMaskRenderer_OnDestroy_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ScopeMaskRenderer).GetMethod(nameof(ScopeMaskRenderer.OnDestroy));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class DistantShadow_OnDestroy_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DistantShadow).GetMethod(nameof(DistantShadow.OnDestroy));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class LevelSettings_SetMotionVectorsToAllTree_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LevelSettings).GetMethod(nameof(LevelSettings.SetMotionVectorsToAllTree));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class PerfectCullingCrossSceneSampler_Initialize_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PerfectCullingCrossSceneSampler).GetMethod(nameof(PerfectCullingCrossSceneSampler.Initialize));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class RenderQueue_UpdateQueue_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(RenderQueue).GetMethod(nameof(RenderQueue.UpdateQueue));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class ClientPasscodeController_SetupHints_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ClientPasscodeController).GetMethod(nameof(ClientPasscodeController.SetupHints));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class AreaLightRenderer_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AreaLightRenderer).GetMethod(nameof(AreaLightRenderer.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class EmissionFlicker_ManualUpdate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EmissionFlicker).GetMethod(nameof(EmissionFlicker.ManualUpdate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
