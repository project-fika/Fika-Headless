using EFT.Interactive;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

public class CarExtraction_Update_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CarExtraction)
            .GetMethod(nameof(CarExtraction.Update));
    }

    [PatchPrefix]
    public static bool Prefix(CarExtraction __instance)
    {
        __instance.Dispose();
        return false;
    }
}
