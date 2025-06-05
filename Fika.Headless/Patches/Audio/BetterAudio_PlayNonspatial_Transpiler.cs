using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio
{
    internal class BetterAudio_PlayNonspatial_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod(nameof(BetterAudio.PlayNonspatial));
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
