using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class WorldInteractiveObject_PlaySound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WorldInteractiveObject).GetMethod(nameof(WorldInteractiveObject.PlaySound), [typeof(EDoorState), typeof(float)]);
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class WorldInteractiveObject_PlayShut_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WorldInteractiveObject).GetMethod(nameof(WorldInteractiveObject.PlayShut));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class Door_PlayOneShotSealSound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Door).GetMethod(nameof(Door.PlayOneShotSealSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class Door_PlayLoopSealSound_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Door).GetMethod(nameof(Door.PlayLoopSealSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
