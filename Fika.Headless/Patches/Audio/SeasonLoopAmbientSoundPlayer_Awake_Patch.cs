using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class SeasonLoopAmbientSoundPlayer_Awake_Patch : ModulePatch
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
