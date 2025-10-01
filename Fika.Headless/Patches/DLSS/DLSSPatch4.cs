using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DLSS;

// Token: 0x02000007 RID: 7
public class DLSSPatch4 : ModulePatch
{
    // Token: 0x06000014 RID: 20 RVA: 0x00002318 File Offset: 0x00000518
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DLSSWrapper).GetConstructor(
        [
            typeof(Material),
            typeof(Material)
        ]);
    }

    // Token: 0x06000015 RID: 21 RVA: 0x00002359 File Offset: 0x00000559
    [PatchPostfix]
    public static void Postfix(SSAAImpl __instance)
    {
        __instance.DLSSDebug = true;
        __instance.DLSSDebugDisable = true;
        __instance.EnableDLSS = false;
    }
}
