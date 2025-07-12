using EFT.UI;
using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class MessageWindow_Show_Patch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MessageWindow)
                .GetMethod(nameof(MessageWindow.Show),
                [typeof(string),
                typeof(string),
                typeof(bool),
                typeof(float)]);
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass3629 __result)
        {
            __result.AcceptAndClose();
            /*__instance.Close(ECloseState.Accept);
            return __result;*/
        }
    }
}
