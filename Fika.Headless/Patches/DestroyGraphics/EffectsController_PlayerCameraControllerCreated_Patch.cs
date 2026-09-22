using EFT;
using EFT.CameraControl;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class EffectsController_PlayerCameraControllerCreated_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController).GetMethod(nameof(EffectsController.PlayerCameraControllerCreated));
    }

    [PatchPrefix]
    public static bool Prefix(EffectsController __instance, PlayerCameraController playerCameraController)
    {
        __instance.player = playerCameraController.Player;
        return false;
    }
}
