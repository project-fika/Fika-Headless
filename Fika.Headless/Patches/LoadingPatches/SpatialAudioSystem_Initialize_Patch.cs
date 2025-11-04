using Audio.SpatialSystem;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.LoadingPatches;

public class SpatialAudioSystem_Initialize_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SpatialAudioSystem)
            .GetMethod(nameof(SpatialAudioSystem.Initialize));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        __result = Task.CompletedTask;
        return false;
    }
}
