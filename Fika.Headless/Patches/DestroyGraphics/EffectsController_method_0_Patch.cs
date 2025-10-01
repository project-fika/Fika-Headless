using EFT;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class EffectsController_method_0_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController).GetMethod(nameof(EffectsController.method_0));
    }

    [PatchPrefix]
    public static bool Prefix(ref Player ___player_0, PlayerCameraController playerCameraController)
    {
        ___player_0 = playerCameraController.Player;
        return false;
    }
}
