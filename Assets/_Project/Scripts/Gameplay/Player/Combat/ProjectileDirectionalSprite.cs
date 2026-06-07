using System.Linq;
using UnityEngine;

namespace Necrocis
{
    public class ProjectileDirectionalSprite : MonoBehaviour
    {
        private const string SpritesheetName = "침 투사체";

        // 8방향 enum (각도 계산 순서)
        private enum Dir8 { N = 0, NE, E, SE, S, SW, W, NW }

        // Dir8 인덱스 → 스프라이트 시트 인덱스
        // N→0(상), NE→1(우상), E→2(우), SE→5(우하), S→7(하), SW→8(좌하), W→6(좌), NW→3(좌상)
        private static readonly int[] DirToSpriteIndex = { 0, 1, 2, 5, 7, 8, 6, 3 };

        [SerializeField] private float spriteScale = 0.2f;

        private Sprite[] sprites;
        private SpriteRenderer sr;

        private void Awake()
        {
            // 자식 오브젝트에 SpriteRenderer 생성 (3D MeshFilter와 충돌 방지)
            GameObject spriteObj = new GameObject("ProjectileSprite");
            spriteObj.transform.SetParent(transform, false);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteObj.transform.localScale = Vector3.one * spriteScale;

            sr = spriteObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2700;

            Billboard bb = spriteObj.AddComponent<Billboard>();
            bb.SetUpdateMode(Billboard.UpdateMode.Continuous);

            // 기존 MeshRenderer 숨기기
            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            LoadSprites();
        }

        private void LoadSprites()
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>(SpritesheetName);
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogWarning($"[ProjectileDirectionalSprite] Resources/{SpritesheetName} 스프라이트 없음 — 파일을 Assets/Resources/ 폴더로 이동하세요.");
                return;
            }

            // 이름 끝 숫자 기준 정렬 (_0, _1 ... _8)
            sprites = loaded.OrderBy(s =>
            {
                string n = s.name;
                int u = n.LastIndexOf('_');
                return u >= 0 && int.TryParse(n.Substring(u + 1), out int idx) ? idx : 999;
            }).ToArray();
        }

        private void OnEnable() => HideSprite();

        public void HideSprite()
        {
            if (sr != null) sr.enabled = false;
        }

        public void SetDirection(Vector3 direction)
        {
            if (sr == null || sprites == null || sprites.Length == 0) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            sr.enabled = true;
            int dir  = GetDir8Index(direction.normalized);
            int sidx = DirToSpriteIndex[dir];
            if (sidx < sprites.Length) sr.sprite = sprites[sidx];
        }

        private static int GetDir8Index(Vector3 dir)
        {
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;

            if (angle < 22.5f  || angle >= 337.5f) return (int)Dir8.E;
            if (angle < 67.5f)                     return (int)Dir8.NE;
            if (angle < 112.5f)                    return (int)Dir8.N;
            if (angle < 157.5f)                    return (int)Dir8.NW;
            if (angle < 202.5f)                    return (int)Dir8.W;
            if (angle < 247.5f)                    return (int)Dir8.SW;
            if (angle < 292.5f)                    return (int)Dir8.S;
            return (int)Dir8.SE;
        }
    }
}
