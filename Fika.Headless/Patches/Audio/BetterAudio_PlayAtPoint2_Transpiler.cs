using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.Audio;

namespace Fika.Headless.Patches.Audio;

internal class BetterAudio_PlayAtPoint2_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPoint),
            [typeof(Vector3), typeof(AudioClip), typeof(float), typeof(BetterAudio.AudioSourceGroupType), typeof(int), typeof(float), typeof(EOcclusionTest), typeof(AudioMixerGroup), typeof(bool)]);
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ldnull);
        yield return new(OpCodes.Ret);
    }
}
