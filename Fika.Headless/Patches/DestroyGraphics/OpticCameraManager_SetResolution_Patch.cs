using EFT.CameraControl;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class OpticCameraManager_SetResolution_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(OpticCameraManager)
            .GetMethod(nameof(OpticCameraManager.SetResolution));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
