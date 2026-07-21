using EFT.Hideout;
using SPT.Reflection.Patching;
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
            .GetMethod(nameof(HideoutRepresentation.Init));
    }

    [PatchPrefix]
    public static bool Prefix(ref Task __result)
    {
        __result = Task.CompletedTask;
        return false;
    }
}
