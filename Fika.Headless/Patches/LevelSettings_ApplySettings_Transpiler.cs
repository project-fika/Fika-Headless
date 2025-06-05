using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    public class LevelSettings_ApplySettings_Transpiler : FikaPatch
    {
        /// <summary>
        /// Prevents unneccesary code from running
        /// </summary>
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LevelSettings).GetMethod(nameof(LevelSettings.ApplySettings));
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
