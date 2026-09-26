using EFT.GameTriggers;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Triggers;

internal class TriggerBallistic_EmitTrigger_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TriggerBallistic).GetMethod(nameof(TriggerBallistic.EmitTrigger));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
