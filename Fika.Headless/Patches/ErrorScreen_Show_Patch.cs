using EFT.UI;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class ErrorScreen_Show_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ErrorScreen).GetMethod(nameof(ErrorScreen.Show),
                [typeof(string), typeof(string), typeof(float), typeof(ErrorScreen.EButtonType), typeof(bool)]);
        }

        [PatchPrefix]
        public static bool Prefix(string message, ref GClass3595 __result)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Logger.LogError("ErrorScreen.Show: " + message);
            }
            else
            {
                Logger.LogWarning("Received an empty error");
            }

            __result = new GClass3595();
            return false;
        }
    }
}
