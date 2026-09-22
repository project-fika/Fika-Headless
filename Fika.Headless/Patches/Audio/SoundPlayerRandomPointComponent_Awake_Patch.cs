using Audio.AmbientSubsystem;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SoundPlayerRandomPointComponent_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SoundPlayerRandomPointComponent).GetMethod(nameof(SoundPlayerRandomPointComponent.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(SoundPlayerRandomPointComponent __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
