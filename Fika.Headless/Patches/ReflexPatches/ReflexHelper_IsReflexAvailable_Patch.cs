using NVIDIA;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.ReflexPatches;

public class ReflexHelper_IsReflexAvailable_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ReflexHelper).GetMethod(nameof(ReflexHelper.IsReflexAvailable),
            [typeof(Reflex.NvReflex_Status).MakeByRefType()]);
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result, out Reflex.NvReflex_Status reflexStatus)
    {
        reflexStatus = Reflex.NvReflex_Status.NvReflex_ERROR;
        __result = false;
        return __result;
    }
}
