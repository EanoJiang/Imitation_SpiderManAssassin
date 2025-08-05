using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    public class ReboneMesh : MonoBehaviour
    {

        public SkinnedMeshRenderer defaultMesh;
        [Space]
        public SkinnedMeshRenderer newMesh;
        public Avatar newAvatar;

#if UNITY_EDITOR
        public void TransferBones()
        {
            defaultMesh.rootBone.parent = transform.root;
            newMesh.transform.parent = defaultMesh.transform.parent;
            newMesh.bones = defaultMesh.bones;
            newMesh.rootBone = defaultMesh.rootBone;

           var anim = defaultMesh.GetComponentInParent<Animator>();

            if (anim && newAvatar != null)
            {
                anim.avatar = newAvatar;
            }

            Debug.Log("Mesh: " + newMesh.name + "has the Bones from "+ defaultMesh.name);
        }
#endif
    }
}