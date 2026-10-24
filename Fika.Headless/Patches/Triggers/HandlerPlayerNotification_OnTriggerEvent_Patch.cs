using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class HandlerPlayerNotification_OnTriggerEvent_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerPlayerNotification).GetMethod(nameof(HandlerPlayerNotification.OnTriggerEvent));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
