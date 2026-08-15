using UnityEngine;

namespace ProceduralMap.Pooling
{
    [DisallowMultipleComponent]
    public sealed class PooledObjectIdentity : MonoBehaviour
    {
        public GameObject SourcePrefab { get; private set; }
        public Vector3 OriginalScale { get; private set; }
        public bool IsInPool { get; set; }

        public void Initialize(GameObject sourcePrefab)
        {
            if (SourcePrefab) return;
            SourcePrefab = sourcePrefab;
            OriginalScale = sourcePrefab.transform.localScale;
        }
    }
}
