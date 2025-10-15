using SPT.Reflection.Patching;
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
    public static bool Prefix(LaserBeam __instance, ref Mesh ___mesh_0, ref Mesh ___mesh_1)
    {
        ___mesh_0 = new();
        ___mesh_1 = new();
        Object.Destroy(__instance);
        return false;
    }
}
