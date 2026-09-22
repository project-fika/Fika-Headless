using NVIDIA;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.ReflexPatches;

public class Reflex_Start_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Reflex)
            .GetMethod(nameof(Reflex.Start));
    }

    [PatchPrefix]
    public static bool Prefix(Reflex __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
