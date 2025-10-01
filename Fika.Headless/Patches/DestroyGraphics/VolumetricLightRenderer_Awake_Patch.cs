using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class VolumetricLightRenderer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(VolumetricLightRenderer).GetMethod(nameof(VolumetricLightRenderer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(VolumetricLightRenderer __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
