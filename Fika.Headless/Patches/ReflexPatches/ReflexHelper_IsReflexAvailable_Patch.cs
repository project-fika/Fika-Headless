using NVIDIA;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace Fika.Headless.Patches.ReflexPatches;

public class ReflexHelper_IsReflexAvailable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ReflexHelper).
            GetMethods().
            FirstOrDefault(x => x.GetParameters().Length == 1);
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result, out Reflex.NvReflex_Status reflexStatus)
    {
        reflexStatus = Reflex.NvReflex_Status.NvReflex_ERROR;
        __result = false;
        return __result;
    }
}
