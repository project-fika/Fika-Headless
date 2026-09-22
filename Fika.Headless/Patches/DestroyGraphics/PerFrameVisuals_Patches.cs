using EFT.Hair;
using Koenigz.PerfectCulling.EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class HairRenderer_LateUpdate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HairRenderer).GetMethod(nameof(HairRenderer.LateUpdate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class HairLights_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HairLights).GetMethod(nameof(HairLights.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class ObservedCullingManager_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ObservedCullingManager).GetMethod(nameof(ObservedCullingManager.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class CullingGridContentSwitcher_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CullingGridContentSwitcher).GetMethod(nameof(CullingGridContentSwitcher.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class CullingGridContentSwitcher_LateUpdate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CullingGridContentSwitcher).GetMethod(nameof(CullingGridContentSwitcher.LateUpdate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class ScreenDistanceSwitcher_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ScreenDistanceSwitcher).GetMethod(nameof(ScreenDistanceSwitcher.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
