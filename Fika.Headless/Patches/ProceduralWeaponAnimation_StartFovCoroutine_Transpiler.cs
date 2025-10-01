using EFT.Animations;
using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches;

/// <summary>
/// This patch aims to decrease the amount of CPU cycles spent updating data the headless does not see
/// </summary>
public class ProceduralWeaponAnimation_StartFovCoroutine_Transpiler : ModulePatch
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
