using NVIDIA;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.ReflexPatches;

public class Reflex_Start_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Reflex)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    [PatchPrefix]
    public static bool Prefix(Reflex __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
