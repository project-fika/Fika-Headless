using EFT;
using Fika.Core.Main.Players;
using SPT.Reflection.Patching;
using Fika.Headless.Classes;
using System.Reflection;

namespace Fika.Headless.Patches;

public class CoopPlayer_CreateMovementContext_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FikaPlayer)
            .GetMethod(nameof(FikaPlayer.CreateMovementContext));
    }

    [PatchPrefix]
    public static bool Prefix(Player __instance)
    {
        if (__instance.IsYourPlayer)
        {
            LayerMask localMask = EFTHardSettings.Instance.MOVEMENT_MASK;
            __instance.MovementContext = HeadlessClientMovementContext.Create(__instance, __instance.GetBodyAnimatorCommon,
                __instance.GetCharacterControllerCommon, localMask);

            return false;
        }

        return true;
    }
}
