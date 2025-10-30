using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.LoadingPatches;

public class StaticDeferredDecalRenderer_UpdateInstancesBuffers_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(StaticDeferredDecalRenderer)
            .GetMethod(nameof(StaticDeferredDecalRenderer.UpdateInstancesBuffers));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
