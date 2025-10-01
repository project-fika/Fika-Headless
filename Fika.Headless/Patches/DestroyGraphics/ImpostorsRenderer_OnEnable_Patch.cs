using EFT.Impostors;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

/// <summary>
/// This patch mostly deals with deleting the tree renderer.
/// </summary>
public class ImpostorsRenderer_OnEnable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ImpostorsRenderer).GetMethod(nameof(ImpostorsRenderer.OnEnable)); // This class is pretty sussy
    }

    [PatchPrefix]
    public static bool Prefix(ImpostorsRenderer __instance)
    {
        Object.Destroy(__instance);
        return false;
    }
}
