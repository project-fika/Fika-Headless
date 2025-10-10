using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class EventLoopPlayer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EventLoopPlayer).GetMethod(nameof(EventLoopPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(EventLoopPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
