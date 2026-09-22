using Audio.AmbientSubsystem;
using Audio.AutoPanner;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class AmbientPlayerAutoPanner_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AmbientPlayerAutoPanner).GetMethod(nameof(AmbientPlayerAutoPanner.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(AmbientPlayerAutoPanner __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
