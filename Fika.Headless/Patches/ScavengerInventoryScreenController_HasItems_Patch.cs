using SPT.Reflection.Patching;
using System.Reflection;
using static EFT.UI.ScavengerInventoryScreen;

namespace Fika.Headless.Patches;

public class ScavengerInventoryScreenController_HasItems_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ScavengerInventoryScreenController)
            .GetProperty(nameof(ScavengerInventoryScreenController.HasItems))
            .GetGetMethod();
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
