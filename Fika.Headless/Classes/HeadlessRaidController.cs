using System;

namespace Fika.Headless.Classes;

/// <summary>
/// Used to sync transforms every frame, as otherwise triggers don't work for e.g. <see cref="EFT.Interactive.TransitPoint"/>s
/// </summary>
public class HeadlessRaidController : MonoBehaviour
{
    public HeadlessRaidController(IntPtr pointer) : base(pointer)
    {
    }

    public void FixedUpdate()
    {
        Physics.SyncTransforms();
    }
}
