using UnityEngine;

namespace Fika.Headless.Classes
{
    /// <summary>
    /// Used to sync transforms every 1 seconds, as otherwise triggers don't work for e.g. <see cref="EFT.Interactive.TransitPoint"/>s
    /// </summary>
    public class HeadlessRaidController : MonoBehaviour
    {
        private readonly float _updateFreq = 1f;
        private float _counter = 0f;

        protected void Update()
        {
            _counter += Time.unscaledDeltaTime;
            if (_counter >= _updateFreq)
            {
                _counter -= _updateFreq;
                Physics.SyncTransforms();
            }
        }
    }
}