using System;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine.Rendering;

namespace Fika.Headless.Patches.DLSS;

/// <summary>
/// Reports Direct3D11 as the graphics device. The client quits on start when the device is not Direct3D11
/// </summary>
public static class GraphicsDeviceTypeIcall
{
    private const string Icall = "UnityEngine.SystemInfo::GetGraphicsDeviceType()";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetGraphicsDeviceTypeDelegate();

    /// <summary>
    /// Kept alive for as long as the engine may call the replacement
    /// </summary>
    private static GetGraphicsDeviceTypeDelegate _replacement;

    /// <summary>
    /// Replace the engine call, which only works before the game first resolves it
    /// </summary>
    /// <param name="logger">Logger to report the result to</param>
    public static void Enable(ManualLogSource logger)
    {
        if (IL2CPP.il2cpp_resolve_icall(Icall) == IntPtr.Zero)
        {
            logger.LogWarning($"{Icall} is not an icall on this build, the client may quit on start");
            return;
        }

        _replacement = () => (int)GraphicsDeviceType.Direct3D11;
        IL2CPP.il2cpp_add_internal_call(Marshal.StringToHGlobalAnsi(Icall), Marshal.GetFunctionPointerForDelegate(_replacement));
        logger.LogInfo("Graphics device reported as Direct3D11");
    }
}
