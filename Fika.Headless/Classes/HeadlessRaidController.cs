namespace Fika.Headless.Classes;

/// <summary>
/// Used to sync transforms every frame, as otherwise triggers don't work for e.g. <see cref="EFT.Interactive.TransitPoint"/>s
/// </summary>
public class HeadlessRaidController : MonoBehaviour
{
    protected void FixedUpdate()
    {
        Physics.SyncTransforms();
    }
}