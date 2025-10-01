using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SeasonAmbientSoundPlayer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SeasonAmbientSoundPlayer).GetMethod(nameof(SeasonAmbientSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(SeasonAmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
