using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.LoadingPatches;

public class AmbientLight_RuntimeOptimizePrepare_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AmbientLight)
            .GetMethod(nameof(AmbientLight.RuntimeOptimizePrepare));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
