// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;
using UnityEngine.Playables;

namespace MxM
{

    public interface IMxMUnityRiggingIntegration
    {
        void Initialize(PlayableGraph a_graph, Animator a_animator);
        void CacheTransforms();
        void FixRigTransforms();
    }
}
