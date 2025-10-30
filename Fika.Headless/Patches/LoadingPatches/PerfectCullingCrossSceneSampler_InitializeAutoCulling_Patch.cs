using Koenigz.PerfectCulling.EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.LoadingPatches;

public class PerfectCullingCrossSceneSampler_InitializeAutoCulling_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PerfectCullingCrossSceneSampler)
            .GetMethod(nameof(PerfectCullingCrossSceneSampler.InitializeAutoCulling));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        __result = Task.CompletedTask;
        return false;
    }
}
