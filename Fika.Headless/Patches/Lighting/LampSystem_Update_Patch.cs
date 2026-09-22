using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Lighting;

internal class LampSystem_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LampSystem).GetMethod(nameof(LampSystem.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}

internal class LampSystem_LateUpdate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LampSystem).GetMethod(nameof(LampSystem.LateUpdate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
