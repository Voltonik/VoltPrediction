using UnityEngine;
using Unity.Netcode;
using Volt.Prediction;

public struct ExampleInputState : IBaseNetworkInputState, INetworkSerializable {
    public int PredictionID { get => m_tick; set => m_tick = value; }
    public Vector2 Movement { get => m_movement; set => m_movement = value; }
    public float Angle { get => m_angle; set => m_angle = value; }

    private int m_tick;
    private Vector2 m_movement;
    private float m_angle;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref m_tick);
        serializer.SerializeValue(ref m_movement);
        serializer.SerializeValue(ref m_angle);
    }
}
public struct ExampleResultState : IBaseNetworkResultState, INetworkSerializable {
    public int PredictionID { get => m_tick; set => m_tick = value; }
    public Vector3 Position { get => m_position; set => m_position = value; }

    private int m_tick;
    private Vector3 m_position;
    public float Angle;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref m_tick);
        serializer.SerializeValue(ref m_position);
        serializer.SerializeValue(ref Angle);
    }
}
