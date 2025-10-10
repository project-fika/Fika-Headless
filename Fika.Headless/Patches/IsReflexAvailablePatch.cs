using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches;

/// <summary>
/// Target the IsReflexAvailable() method with no parameters
/// </summary>
public class IsReflexAvailablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3692)
            .GetMethod(nameof(GClass3692.IsReflexAvailable),
            BindingFlags.Public | BindingFlags.Static,
            null, [], null);
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
