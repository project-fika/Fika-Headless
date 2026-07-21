using EFT;
using EFT.Ballistics;
using EFT.Vehicle;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.PhysicsPatches;

/// <summary>
/// This patch syncs all transforms before a bullet checks if it hits
/// </summary>
internal class Shot_CheckCollision_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Shot)
            .GetMethod(nameof(Shot.CheckCollision), [typeof(Vector3), typeof(Vector3)]);
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo method = typeof(Physics).GetMethod(nameof(Physics.SyncTransforms));
        yield return new(OpCodes.Call, method);

        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;
        }
    }
}
