using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class LoopAmbientSoundPlayer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LoopAmbientSoundPlayer).GetMethod(nameof(LoopAmbientSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(LoopAmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
