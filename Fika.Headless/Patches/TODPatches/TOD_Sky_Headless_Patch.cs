using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.TODPatches;

internal class TOD_Sky_Headless_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TOD_Sky)
            .GetProperty(nameof(TOD_Sky.Headless))
            .GetGetMethod();
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}
