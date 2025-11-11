/*using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DebugPatches;

[DebugPatch]
internal class LoadingScreenDebugPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_1));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        Logger.LogInfo($"Loading Map: {pr}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_2));
    }

    [PatchPrefix]
    public static bool Prefix(float totalProgress)
    {
        Logger.LogInfo($"Caching Data: {totalProgress}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch3 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_3));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        Logger.LogInfo($"Loading Auto-Culling: {pr}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch4 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_4));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        Logger.LogInfo($"Loading Spatial Audio: {pr}%");
        return false;
    }
}*/