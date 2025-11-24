using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DebugPatches;

[DebugPatch]
internal class LoadingScreenDebugPatch : ModulePatch
{
    private static int _lastProgress = -1;
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_1));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        if (!FikaHeadlessPlugin.ShowDebugLogging.Value) return false;
        pr *= 100;
        if ((int)pr == _lastProgress) return false;
        _lastProgress = (int)pr;
        Logger.LogInfo($"Loading Map: {(int)pr}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch2 : ModulePatch
{
    private static int _lastProgress = -1;
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_2));
    }

    [PatchPrefix]
    public static bool Prefix(float totalProgress)
    {
        if (!FikaHeadlessPlugin.ShowDebugLogging.Value) return false;
        totalProgress *= 100;
        if ((int)totalProgress == _lastProgress) return false;
        _lastProgress = (int)totalProgress;
        Logger.LogInfo($"Caching Data: {totalProgress}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch3 : ModulePatch
{
    private static int _lastProgress = -1;
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_3));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        if (!FikaHeadlessPlugin.ShowDebugLogging.Value) return false;
        pr *= 100;
        if ((int)pr == _lastProgress) return false;
        _lastProgress = (int)pr;
        Logger.LogInfo($"Loading Auto-Culling: {pr}%");
        return false;
    }
}

[DebugPatch]
internal class LoadingScreenDebugPatch4 : ModulePatch
{
    private static int _lastProgress = -1;
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TarkovApplication.Class1505).
            GetMethod(nameof(TarkovApplication.Class1505.method_4));
    }

    [PatchPrefix]
    public static bool Prefix(float pr)
    {
        if (!FikaHeadlessPlugin.ShowDebugLogging.Value) return false;
        pr *= 100;
        if ((int)pr == _lastProgress) return false;
        _lastProgress = (int)pr;
        Logger.LogInfo($"Loading Spatial Audio: {pr}%");
        return false;
    }
}