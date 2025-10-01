using SPT.Reflection.Patching;
using MultiFlare;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

internal class FlareSceneSettings_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FlareSceneSettings).GetMethod(nameof(FlareSceneSettings.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(FlareSceneSettings __instance)
    {
        MonoBehaviour.Destroy(__instance);
        return false;
    }
}
