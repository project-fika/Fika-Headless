using EFT.CameraControl;
using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.VRAM;

// Token: 0x0200000B RID: 11
public class VRAMPatch3 : ModulePatch
{
    // Token: 0x06000020 RID: 32 RVA: 0x000024BC File Offset: 0x000006BC
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CameraManager).GetMethod("GetVRamUsage");
    }

    // Token: 0x06000021 RID: 33 RVA: 0x000024E4 File Offset: 0x000006E4
    [PatchPrefix]
    public static bool Prefix(ref ulong totalVRam, ref ulong localBudget, ref ulong localCurrentUsage)
    {
        totalVRam = 0;
        localBudget = 0;
        localCurrentUsage = 0;
        return false;
    }
}
