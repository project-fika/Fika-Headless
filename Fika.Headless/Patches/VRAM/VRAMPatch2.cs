using Fika.Core.Patching;
using HarmonyLib;
using System;
using System.Reflection;

namespace Fika.Headless.Patches.VRAM;

// Token: 0x0200000A RID: 10
public class VRAMPatch2 : FikaPatch
{
    // Token: 0x0600001D RID: 29 RVA: 0x00002440 File Offset: 0x00000640
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CameraClass).GetMethod("SetCamera");
    }

    // Token: 0x0600001E RID: 30 RVA: 0x00002468 File Offset: 0x00000668
    [PatchPrefix]
    public static bool Prefix(CameraClass __instance, Camera camera)
    {
        __instance.Reset();
        __instance.Camera = camera;
        __instance.SetOcclusionCullingEnabled(false);
        __instance.method_2();
        Action action = Traverse.Create(__instance).Field<Action>("action_1").Value;
        if (action != null)
        {
            action();
        }
        return false;
    }
}
