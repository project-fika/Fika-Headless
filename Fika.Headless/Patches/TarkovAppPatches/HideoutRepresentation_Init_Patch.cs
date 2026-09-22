using EFT;
using EFT.Hideout;
using SPTushonka.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Fika.Headless.Patches.TarkovAppPatches;

/// <summary>
/// Stops the headless from loading the hideout
/// </summary>
public class HideoutRepresentation_Init_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HideoutRepresentation)
            .GetMethod(nameof(HideoutRepresentation.Init), [typeof(IEftSession)]);
    }

    [PatchPrefix]
    public static bool Prefix(ref Il2CppSystem.Threading.Tasks.Task __result)
    {
        __result = Il2CppSystem.Threading.Tasks.Task.CompletedTask;
        return false;
    }
}
