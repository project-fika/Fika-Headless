using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.WeaponManagerClassPatches;

public class WeaponManagerClass_method_3_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(WeaponManagerClass)
            .GetMethod(nameof(WeaponManagerClass.method_3));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return __result;
    }
}