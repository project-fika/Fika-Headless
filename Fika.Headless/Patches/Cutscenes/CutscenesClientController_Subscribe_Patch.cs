using CommonAssets.Scripts.Cutscenes;
using HarmonyLib;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.Cutscenes;

internal class CutscenesClientController_Subscribe_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(CutscenesClientController), "Subscribe");
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
