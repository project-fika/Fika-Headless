using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class WindowBreaker_PlayHitSound_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WindowBreaker)
            .GetMethod(nameof(WindowBreaker.PlayHitSound));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
