using Audio.AmbientSubsystem;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class AmbientSoundPlayer_Awake_Patch : FikaPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(AmbientSoundPlayer).GetMethod(nameof(AmbientSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(AmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
