using EFT.Hideout;
using EFT;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.TarkovAppPatches;

/// <summary>
/// Stops the <see cref="MainMenuShowOperation"/> from trying to set the hideout inventory
/// </summary>
public class MainMenuShowOperation_CG_Init_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HideoutRepresentation).GetMethod(nameof(HideoutRepresentation.SetupControllers));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
