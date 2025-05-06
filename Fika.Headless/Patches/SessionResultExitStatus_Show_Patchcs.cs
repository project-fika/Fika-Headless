using EFT;
using EFT.UI;
using EFT.UI.SessionEnd;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
namespace Fika.Headless.Patches
{
    public class SessionResultExitStatusPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(SessionResultExitStatus).GetMethod(nameof(SessionResultExitStatus.Show),
                [typeof(Profile), typeof(LastPlayerStateClass), typeof(ESideType),
                typeof(ExitStatus), typeof(TimeSpan), typeof(ISession), typeof(bool)]);
        }

        [PatchPostfix]
        public static void PatchPostfix(DefaultUIButton ____mainMenuButton)
        {
            ____mainMenuButton.OnClick.Invoke();
            FikaHeadlessPlugin.Instance.OnSessionResultExitStatus_Show();
        }
    }
}
