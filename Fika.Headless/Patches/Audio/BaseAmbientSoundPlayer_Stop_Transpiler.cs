using Audio.AmbientSubsystem;
using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.Audio
{
    internal class BaseAmbientSoundPlayer_Stop_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseAmbientSoundPlayer).GetMethod(nameof(BaseAmbientSoundPlayer.Stop));
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
