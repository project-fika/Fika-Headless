using Audio.AmbientSubsystem;
using Audio.AutoPanner;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class AmbientPlayerAutoPanner_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AmbientPlayerAutoPanner).GetMethod(nameof(AmbientPlayerAutoPanner.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(AmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
