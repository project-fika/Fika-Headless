using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.FirearmsPatches;

public class Firearms_IsShellVisible_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Firearms)
            .GetMethod(nameof(Firearms.IsShellVisible));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return __result;
    }
}