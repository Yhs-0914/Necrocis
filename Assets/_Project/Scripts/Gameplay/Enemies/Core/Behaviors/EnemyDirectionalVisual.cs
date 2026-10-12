using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
        private EnemyDirectionalSnapshot patternDirections;
        private EnemyFacing patternFacing = EnemyFacing.Front;
        private bool patternFacingInitialized, patternFacingLocked;
        private Sprite patternSourceFrame;

        public bool HasPatternDirections => patternDirections != null;
        public EnemyFacing PatternFacing => patternFacing;
        public bool IsPatternFacingLocked => patternFacingLocked;

        public void BindPatternDirections(EnemyDirectionalPresentation presentation)
        {
            if (deathAnimPlaying || IsDead) throw new System.InvalidOperationException("사망 재생 중에는 방향 원본을 교체할 수 없습니다.");
            EnemyDirectionalSnapshot snapshot = presentation != null ? presentation.Capture() : null;
            if (snapshot != null && config?.deathSprites != null)
                foreach (Sprite source in config.deathSprites) snapshot.Resolve(source, EnemyFacing.Front);
            ResetPatternDirections();
            patternDirections = snapshot;
            if (snapshot != null && spriteRenderer != null) spriteRenderer.flipX = false;
        }

        public void CommitPatternFacing(Vector3 direction, bool authoredFacingLeft = true)
        {
            SetPatternFacing(direction, authoredFacingLeft);
            patternFacingLocked = true;
        }

        public void ReleasePatternFacing()
        {
            if (!deathAnimPlaying && !IsDead) patternFacingLocked = false;
        }

        public Vector3 GetPatternVisualOrigin()
        {
            if (patternDirections == null || visualRoot == null) return transform.position;
            Vector2 origin = patternDirections.Origin(patternFacing);
            return visualRoot.TransformPoint(new Vector3(origin.x, origin.y, 0));
        }

        private void ResetPatternDirections()
        {
            patternDirections = null;
            patternFacing = EnemyFacing.Front;
            patternFacingInitialized = patternFacingLocked = false;
            patternSourceFrame = null;
        }

        private bool ApplyPatternDirection(Vector3 direction)
        {
            if (patternDirections == null) return false;
            patternFacing = patternDirections.Select(direction, DontStarveCamera.GetActiveCamera(), patternFacing, patternFacingInitialized);
            if (Vector3.ProjectOnPlane(direction, Vector3.up).sqrMagnitude >= .000001f) patternFacingInitialized = true;
            spriteRenderer.flipX = patternDirections.Flip(patternFacing);
            if (patternSourceFrame != null) spriteRenderer.sprite = patternDirections.Resolve(patternSourceFrame, patternFacing);
            return true;
        }

        private Sprite[] ResolvePatternDeathFrames(Sprite[] source)
        {
            if (patternDirections == null || source == null) return source;
            var result = new Sprite[source.Length];
            for (int i = 0; i < source.Length; i++) result[i] = patternDirections.Resolve(source[i], patternFacing);
            spriteRenderer.flipX = patternDirections.Flip(patternFacing);
            return result;
        }
    }
}
