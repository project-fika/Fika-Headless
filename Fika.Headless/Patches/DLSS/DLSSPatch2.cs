using Fika.Core.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DLSS;

// Token: 0x02000005 RID: 5
public class DLSSPatch2 : FikaPatch
{
    // Token: 0x0600000E RID: 14 RVA: 0x00002288 File Offset: 0x00000488
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DLSSWrapper).GetMethod("IsDLSSLibraryLoaded");
    }

    // Token: 0x0600000F RID: 15 RVA: 0x000022B0 File Offset: 0x000004B0
    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
