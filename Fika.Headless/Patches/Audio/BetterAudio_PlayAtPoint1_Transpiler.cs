using EFT;
using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_PlayAtPoint1_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayAtPoint),
                [typeof(Vector3), typeof(SoundBank), typeof(float), typeof(float), typeof(float), typeof(EnvironmentType), typeof(EOcclusionTest), typeof(bool)]);
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            // Create a new set of instructions
            List<CodeInstruction> instructionsList =
            [
                new CodeInstruction(OpCodes.Ret) // Return immediately
            ];

            return instructionsList;
        }
    }
}
