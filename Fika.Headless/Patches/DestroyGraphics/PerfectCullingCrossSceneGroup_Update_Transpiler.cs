using SPT.Reflection.Patching;
using HarmonyLib;
using Koenigz.PerfectCulling.EFT;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class PerfectCullingCrossSceneGroup_Update_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PerfectCullingCrossSceneGroup).GetMethod(nameof(PerfectCullingCrossSceneGroup.Update));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ret);
    }
}
