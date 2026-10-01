using System.Reflection;
using EFT.CameraControl;
using SPT.Reflection.Patching;

namespace Fika.Headless.Patches.DestroyGraphics;

public sealed class OpticComponentUpdater_LateUpdate_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(OpticComponentUpdater)
            .GetMethod(nameof(OpticComponentUpdater.LateUpdate));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
