// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using Unity.Mathematics;

namespace MxM
{
    public struct TransformData
    {
        public float3 Position;
        public quaternion Rotation;

        public TransformData(float3 a_position, quaternion a_rotation)
        {
            Position = a_position;
            Rotation = a_rotation;
        }

        public void SetPositionAndRotation(float3 a_position, quaternion a_rotation)
        {
            Position = a_position;
            Rotation = a_rotation;
        }
    }
}
