using EFT.Tripwire;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio.Tripwire;

internal class TripwireSoundController_PlayFuzeSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TripwireSoundController)
            .GetMethod(nameof(TripwireSoundController.PlayFuzeSound));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
