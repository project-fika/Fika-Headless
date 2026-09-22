using SPTushonka.Reflection.Patching;
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

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
