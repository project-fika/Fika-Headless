using SPT.Reflection.Patching;
using System.Reflection;
using WaterSSR;

namespace Fika.Headless.Patches.DestroyGraphics;

public class WaterRendererv3_OnEnable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WaterRendererv3).GetMethod(nameof(WaterRendererv3.OnEnable));
    }

    [PatchPrefix]
    public static bool Prefix(WaterRendererv3 __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
