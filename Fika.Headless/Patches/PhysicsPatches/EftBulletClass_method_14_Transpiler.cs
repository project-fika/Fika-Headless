using SPT.Reflection.Patching;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.PhysicsPatches;

/// <summary>
/// This patch syncs all transforms before a bullet checks if it hits
/// </summary>
internal class EftBulletClass_method_14_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EftBulletClass)
            .GetMethod(nameof(EftBulletClass.method_14));
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
