using Fika.Core.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// The purpose of this patch is to disable bot sleeping on the headless host
    /// </summary>
    [IgnoreAutoPatch]
    public class BotStandBy_Update_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotStandBy).GetMethod(nameof(BotStandBy.Update));
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
