using Audio.Vehicles.BTR;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.BTR;

/// <summary>
/// Prevents a nullref on headless due audio not processing
/// </summary>
public class BtrSoundController_UpdateImpactPlayers_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BtrSoundController)
            .GetMethod(nameof(BtrSoundController.Update));
    }

    [PatchPrefix]
    public static bool Prefix(GInterface83 ____phraseController)
    {
        ____phraseController.ManualUpdate(0F);
        return false;
    }
}
