using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class LocalDustParticlesParent_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocalDustParticlesParent).GetMethod(nameof(LocalDustParticlesParent.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(LocalDustParticlesParent __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
