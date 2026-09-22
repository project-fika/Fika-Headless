using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class WindowBreaker_PlayPieceSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WindowBreaker)
            .GetMethod(nameof(WindowBreaker.PlayPieceSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
