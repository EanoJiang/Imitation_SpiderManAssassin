// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;

namespace MxM
{
    [CreateAssetMenu(fileName = "MxMWarpDataModule", menuName = "MxM/Utility/MxMWarpDataModule", order = 0)]
    public class WarpModule : ScriptableObject
    {
        public EAngularErrorWarp AngularErrorWarpType = EAngularErrorWarp.On;
        public EAngularErrorWarpMethod AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
        public float WarpRate = 60f;
        public float DistanceThreshold = 1f;
        public Vector2 AngleRange = new Vector2(0.5f, 90f);
        public ELongitudinalErrorWarp LongErrorWarpType = ELongitudinalErrorWarp.None;
        public Vector2 LongWarpSpeedRange = new Vector2(0.9f, 1.2f);
    }
}