using Fika.Core.Patching;
using System.Reflection;


namespace Fika.Headless.Patches.DestroyGraphics;

public class ScopeMaskRenderer_Awake_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ScopeMaskRenderer).GetMethod(nameof(ScopeMaskRenderer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(ScopeMaskRenderer __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
