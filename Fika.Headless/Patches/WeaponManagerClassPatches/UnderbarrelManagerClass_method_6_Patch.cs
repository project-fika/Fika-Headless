using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.WeaponManagerClassPatches;

internal class UnderbarrelManagerClass_method_6_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.FirearmController.UnderbarrelManagerClass)
            .GetMethod(nameof(Player.FirearmController.UnderbarrelManagerClass.method_6));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return __result;
    }
}
