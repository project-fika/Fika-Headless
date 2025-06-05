using HarmonyLib;
using JsonType;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_PlayDropItem2_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayDropItem), [typeof(BaseBallistic.ESurfaceSound), typeof(EItemDropSoundType), typeof(Vector3), typeof(float)]);
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
