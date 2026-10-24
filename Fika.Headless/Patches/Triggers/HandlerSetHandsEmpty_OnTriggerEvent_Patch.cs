using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class HandlerSetHandsEmpty_OnTriggerEvent_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerSetHandsEmpty).GetMethod(nameof(HandlerSetHandsEmpty.OnTriggerEvent));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
