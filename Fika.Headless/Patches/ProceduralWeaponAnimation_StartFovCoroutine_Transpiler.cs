using EFT.Animations;
using HarmonyLib;
using Fika.Core.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
    /// </summary>
    public class ProceduralWeaponAnimation_StartFovCoroutine_Transpiler : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ProceduralWeaponAnimation).GetMethod(nameof(ProceduralWeaponAnimation.StartFovCoroutine));
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
