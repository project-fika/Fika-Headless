using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class MessageWindow_Show_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MessageWindow).GetMethod(nameof(MessageWindow.Show), [typeof(string),
                typeof(string),
                typeof(bool),
                typeof(float)]);
        }

        [PatchPostfix]
        public static void PatchPostfix(GClass3594 __result, MessageWindow __instance)
        {
            __result.AcceptAndClose();
            /*__instance.Close(ECloseState.Accept);
            return __result;*/
        }
    }
}
