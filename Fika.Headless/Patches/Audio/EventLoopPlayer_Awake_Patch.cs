using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class BaseRandomAmbientSoundPlayer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BaseRandomAmbientSoundPlayer).GetMethod(nameof(BaseRandomAmbientSoundPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(BaseAmbientSoundPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
