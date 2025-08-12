using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SeasonLoopAmbientSoundPlayer_Awake_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SeasonLoopAmbientSoundPlayer).GetMethod(nameof(SeasonLoopAmbientSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(SeasonLoopAmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
