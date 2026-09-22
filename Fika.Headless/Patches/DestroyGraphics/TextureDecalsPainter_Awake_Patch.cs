using SPTushonka.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics;

public class TextureDecalsPainter_Awake_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TextureDecalsPainter)
            .GetMethod(nameof(TextureDecalsPainter.Awake));
    }

    [PatchPrefix]
    public static bool Prefix(TextureDecalsPainter __instance)
    {
        __instance._texturesPool = new ObjectPool<RenderTexture>(0, new System.Func<RenderTexture>(FakeClassFunc));
        Object.Destroy(__instance);
        return false;
    }

    private static RenderTexture FakeClassFunc()
    {
        return new();
    }
}
