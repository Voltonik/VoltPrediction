using UnityEngine;
using Unity.Netcode;

namespace Volt.Prediction {
    public interface IBaseNetworkInputState : INetworkSerializable {
        public int PredictionID { get; set; }
        public Vector2 Movement { get; set; }
    }

    public interface IBaseNetworkResultState : INetworkSerializable {
        public int PredictionID { get; set; }
        public Vector3 Position { get; set; }
    }
}