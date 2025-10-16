using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class StaticDeferredDecal_OnEnable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(StaticDeferredDecal)
            .GetMethod(nameof(StaticDeferredDecal.OnEnable));
    }

    [PatchPrefix]
    public static bool Prefix(StaticDeferredDecal __instance)
    {
        Object.Destroy(__instance.gameObject);
        Object.Destroy(__instance);
        return false;
    }
}
