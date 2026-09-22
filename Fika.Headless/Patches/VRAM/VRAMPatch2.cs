using EFT.CameraControl;
using HarmonyLib;
using SPTushonka.Reflection.Patching;
using System;
using System.Reflection;

namespace Fika.Headless.Patches.VRAM;

// Token: 0x0200000A RID: 10
public class VRAMPatch2 : ModulePatch
{
    // Token: 0x0600001D RID: 29 RVA: 0x00002440 File Offset: 0x00000640
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CameraManager).GetMethod("SetCamera");
    }

    // Token: 0x0600001E RID: 30 RVA: 0x00002468 File Offset: 0x00000668
    [PatchPrefix]
    public static bool Prefix(CameraManager __instance, Camera camera)
    {
        __instance.Reset();
        __instance.Camera = camera;
        __instance.InitCamera();
        __instance.OnCameraChangedField?.Invoke();
        return false;
    }
}
