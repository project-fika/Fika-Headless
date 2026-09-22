using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class LaserBeam_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LaserBeam)
            .GetMethod(nameof(LaserBeam.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(LaserBeam __instance)
    {
        __instance._pointMesh = new Mesh();
        __instance._beamMesh = new Mesh();
        Object.Destroy(__instance);
        return false;
    }
}
