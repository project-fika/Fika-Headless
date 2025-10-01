using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class EventRandomPlayer_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EventRandomPlayer).GetMethod(nameof(EventRandomPlayer.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(EventRandomPlayer __instance)
    {
        GameObject.Destroy(__instance);
        return false;
    }
}
