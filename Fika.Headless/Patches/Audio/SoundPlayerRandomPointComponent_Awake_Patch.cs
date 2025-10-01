using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SoundPlayerRandomPointComponent_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SoundPlayerRandomPointComponent).GetMethod(nameof(SoundPlayerRandomPointComponent.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(AmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
