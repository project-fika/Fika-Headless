using EFT.GameTriggers;
using EFT.Interactive;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class HandlerTransitTimeWindow_OnTrigger_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerTransitTimeWindow).GetMethod(nameof(HandlerTransitTimeWindow.OnTrigger));
    }

    [PatchPrefix]
    public static bool Prefix(HandlerTransitTimeWindow __instance, TriggerEvent triggerEvent)
    {
        var requirement = __instance._timeWindowTransitRequirement;
        if (!__instance._showNotificationOnFail || requirement == null
            || TimeWindowRequirement.CheckGameTime(requirement.FromHour, requirement.ToHour))
        {
            return true;
        }

        if (__instance.HasAuthority())
        {
            TriggersEmitter.Instance.Emit(__instance._outputOnFailedTriggerId, triggerEvent.OriginPlayerRaidId);
        }

        return false;
    }
}
