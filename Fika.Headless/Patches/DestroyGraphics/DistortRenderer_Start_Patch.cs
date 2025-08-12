using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class DistortRenderer_Start_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DistortRenderer).GetMethod(nameof(DistortRenderer.Start));
    }

    [PatchPrefix]
    public static bool Prefix(DistortRenderer __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
