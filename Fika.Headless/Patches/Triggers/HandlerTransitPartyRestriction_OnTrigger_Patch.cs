using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class HandlerTransitPartyRestriction_OnTrigger_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerTransitPartyRestriction).GetMethod(nameof(HandlerTransitPartyRestriction.OnTrigger));
    }

    [PatchPrefix]
    public static void Prefix(HandlerTransitPartyRestriction __instance)
    {
        __instance._showNotificationOnFail = false;
    }
}
