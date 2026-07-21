using EFT;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class EffectsController_PlayerCameraControllerCreated_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController).GetMethod(nameof(EffectsController.PlayerCameraControllerCreated));
    }

    [PatchPrefix]
    public static bool Prefix(ref Player ___player, PlayerCameraController playerCameraController)
    {
        ___player = playerCameraController.Player;
        return false;
    }
}
