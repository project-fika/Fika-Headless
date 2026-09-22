using EFT.Tripwire;
using HarmonyLib;
using SPTushonka.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio.Tripwire;

internal class TripwireSoundController_PlayPinSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TripwireSoundController)
            .GetMethod(nameof(TripwireSoundController.PlayPinSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
