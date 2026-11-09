using UnityEngine;

namespace InfiniteAdventure
{
    public struct VfxRequest
    {
        public VfxDataSO Data;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [CreateAssetMenu(menuName = "Game/Events/Vfx Request")]
    public class VfxRequestEventChannelSO : EventChannelSO<VfxRequest> { }
}
