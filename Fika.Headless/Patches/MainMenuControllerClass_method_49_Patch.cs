using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    /// <summary>
    /// This patch simulates clicking "Next" by calling the method bound to the event of the button <br/>
    /// <see cref="EFT.UI.Matchmaker.MatchmakerOfflineRaidScreen.CreateRaidSettingsForProfileClass"/>
    /// </summary>
    internal class MainMenuControllerClass_method_49_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuControllerClass)
                .GetMethod(nameof(MainMenuControllerClass.method_49));
        }

        [PatchPrefix]
        public static bool Prefix(MainMenuControllerClass __instance)
        {
            __instance.method_81();
            return false;
        }
    }
}
