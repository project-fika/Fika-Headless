using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class HandlerPlantAction_OnTriggerEvent_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HandlerPlantAction).GetMethod(nameof(HandlerPlantAction.OnTriggerEvent));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
