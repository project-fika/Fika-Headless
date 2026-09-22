using Audio.NPC.BtrDriver;
using Audio.Vehicles.BTR;
using SPTushonka.Reflection.Patching;
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
    public static bool Prefix()
    {
        return false;
    }
}
