using SPTushonka.Reflection.Patching;
using Audio.AudioCulling;
using System.Reflection;

namespace Fika.Headless.Patches.Audio;

internal class AudioSourceCulling_StartWorkCoroutine_Transpiler : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BaseAudioCulling).GetMethod(nameof(BaseAudioCulling.StartWorkCoroutine));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        return false;
    }
}
