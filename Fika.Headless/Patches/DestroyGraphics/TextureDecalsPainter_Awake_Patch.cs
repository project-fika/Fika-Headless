using SPT.Reflection.Patching;
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
    public static bool Prefix(TextureDecalsPainter __instance, ref ObjectPool<RenderTexture> ____texturesPool)
    {
        ____texturesPool = new(0, FakeClassFunc);
        Object.Destroy(__instance);
        return false;
    }

    private static RenderTexture FakeClassFunc()
    {
        return new();
    }
}
