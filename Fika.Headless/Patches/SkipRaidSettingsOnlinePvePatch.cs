using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

public class SkipRaidSettingsOnlinePvePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(RaidSettings).GetMethod(nameof(RaidSettings.UpdateOnlinePveRaidStates));
    }

    [PatchPrefix]
    public static bool PatchPrefix()
    {
        return false;
    }
}
