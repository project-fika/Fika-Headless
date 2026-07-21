using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.FirearmsPatches;

internal class UnderbarrelContainer_IsShellVisible_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.FirearmController.UnderbarrelContainer)
            .GetMethod(nameof(Player.FirearmController.UnderbarrelContainer.IsShellVisible));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return __result;
    }
}
