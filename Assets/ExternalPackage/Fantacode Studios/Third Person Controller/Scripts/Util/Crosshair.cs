using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_ThirdPerson
{

    public class Crosshair : MonoBehaviour
    {
        public float size = 1.0f;
        private Camera mainCamera;
        private SpriteRenderer spriteRenderer;
        private MeshRenderer meshRenderer;

        void Start()
        {
            mainCamera = Camera.main;
            spriteRenderer = GetComponent<SpriteRenderer>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Update()
        {
            transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
            float scaleFactor = (distance / 5) * size;
            var y = spriteRenderer != null ? spriteRenderer.sprite.bounds.size.y : GetMeshBoundsSize();
            transform.localScale = Vector3.one * scaleFactor / y;
        }

        public float GetMeshBoundsSize()
        {
            if (meshRenderer != null && meshRenderer.bounds.size != Vector3.zero)
                return meshRenderer.bounds.size.y;
            return 1.0f;
        }
    }
}