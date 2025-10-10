using SPT.Reflection.Patching;
using System.Reflection;


namespace Fika.Headless.Patches.DestroyGraphics;

public class ScopeMaskRenderer_Awake_Patch : ModulePatch
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
