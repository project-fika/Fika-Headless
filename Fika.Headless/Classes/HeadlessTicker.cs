using System;

namespace Fika.Headless.Classes;

public sealed class HeadlessTicker : MonoBehaviour
{
    public HeadlessTicker(IntPtr pointer) : base(pointer)
    {
    }

    public void Update()
    {
        FikaHeadlessPlugin.Instance?.Tick();
    }
}
