using EFT.UI;
using SPTushonka.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace Fika.Headless.Patches;

public class MessageWindow_Show_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemUiContext)
            .GetMethods()
            .First(x => x.Name == nameof(ItemUiContext.ShowMessageWindow)
                && x.ReturnType == typeof(DialogWindowContext)
                && x.GetParameters()[0].ParameterType == typeof(MessageWindow));
    }

    [PatchPostfix]
    public static void PatchPostfix(DialogWindowContext __result)
    {
        __result?.AcceptAndClose();
        /*__instance.Close(ECloseState.Accept);
        return __result;*/
    }
}
