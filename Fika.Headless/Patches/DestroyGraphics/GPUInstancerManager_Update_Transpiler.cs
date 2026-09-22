using SPTushonka.Reflection.Patching;
using GPUInstancer;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Fika.Headless.Patches.DestroyGraphics;

public class GPUInstancerManager_Update_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GPUInstancerManager).GetMethod(nameof(GPUInstancerManager.Update));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
