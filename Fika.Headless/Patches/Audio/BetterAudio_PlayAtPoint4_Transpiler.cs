using EFT;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_PlayAtPoint4_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPoint),
            [typeof(Vector3), typeof(SoundBank), typeof(int), typeof(float), typeof(float),
            typeof(float), typeof(EnvironmentType), typeof(EOcclusionTest), typeof(bool), typeof(bool), typeof(bool)]);
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        yield return new(OpCodes.Ldnull);
        yield return new(OpCodes.Ret);
    }
}
