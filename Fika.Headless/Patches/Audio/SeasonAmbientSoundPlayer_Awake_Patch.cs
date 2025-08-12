using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SeasonAmbientSoundPlayer_Awake_Patch : FikaPatch
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
