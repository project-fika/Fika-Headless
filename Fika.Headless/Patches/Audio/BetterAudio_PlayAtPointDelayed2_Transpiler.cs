using EFT;
using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Audio;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_PlayAtPointDelayed2_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPointDelayed),
                [typeof(Vector3), typeof(SoundBank), typeof(BetterAudio.AudioSourceGroupType), typeof(float), typeof(float),
                typeof(float), typeof(float), typeof(EnvironmentType), typeof(EOcclusionTest), typeof(bool), typeof(AudioMixerGroup)]);
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile()
        {
            yield return new(OpCodes.Ldnull);
            yield return new(OpCodes.Ret);
        }
    }
}
