using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch ensures that the raid settings are skipped if you are playing as a scav
    /// </summary>
    internal class MainMenuControllerClass_method_76_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass).GetMethod(nameof(MainMenuControllerClass.method_76));
        }

        [PatchPostfix]
        public static void Postfix(MainMenuControllerClass __instance, RaidSettings ___RaidSettings_0)
        {
            if (___RaidSettings_0.IsScav)
            {
                __instance.method_49();
            }
        }
    }
}
