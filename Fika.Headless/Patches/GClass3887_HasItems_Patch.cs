using SPT.Reflection.Patching;
using System.Reflection;
using static EFT.UI.ScavengerInventoryScreen;

namespace Fika.Headless.Patches;

public class GClass3887_HasItems_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3887)
            .GetProperty(nameof(GClass3887.HasItems))
            .GetGetMethod();
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
