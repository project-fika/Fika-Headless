using EFT.Interactive;
using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// Attempts to fix a nullref due to renders being disabled
    /// </summary>
    public class WindowBreaker_method_11_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WindowBreaker).GetMethod(nameof(WindowBreaker.method_11));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            // Create a new set of instructions
            List<CodeInstruction> instructionsList = [.. instructions];

            for (int i = 2; i < 7; i++)
            {
                instructionsList[i].opcode = OpCodes.Nop;
            }

            for (int i = 16; i < 29; i++)
            {
                instructionsList[i].opcode = OpCodes.Nop;
            }

            return instructionsList;
        }
    }
}
