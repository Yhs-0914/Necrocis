using UnityEngine;

namespace Necrocis
{
    /// <summary>
    /// Necrocis 플레이어를 절차적 장기 맵의 절벽/층/용암 이동 규칙과 연결한다.
    /// 캐릭터 전투와 애니메이션은 기존 PlayerController가 계속 담당한다.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class ProceduralTerrainMotor : MonoBehaviour
    {
        [SerializeField] private Vector2 terrainHalfExtents = new Vector2(0.68f, 0.48f);
        [SerializeField, Min(0.1f)] private float holdDuration = 2f;
        [SerializeField, Min(0.05f)] private float climbMoveDuration = 0.18f;
        [SerializeField, Min(0.1f)] private float lavaJumpDuration = 0.5f;
        [SerializeField, Min(0f)] private float lavaJumpHeight = 0.8f;
        [SerializeField, Min(0f)] private float heightStep = 0.5f;

        private PlayerController player;
        private MovementExecutionLease traversalExecution;
        private IPlayerTerrainQueries terrain;
        private float baseWorldY;
        private float holdTimer;
        private Vector3 pendingDestination;
        private bool pendingLavaJump;
        private bool hasPendingDestination;
        private bool isTraversing;
        private float traversalTimer;
        private float traversalDuration;
        private Vector3 traversalStart;
        private Vector3 traversalDestination;

        public bool HasActiveMap => terrain != null && terrain.IsReady;
        public bool IsTraversing => isTraversing;
        public Vector2 TerrainHalfExtents => terrainHalfExtents;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            BindMapIfNeeded();
            if (player != null && player.IsBossDisplaced) { ResetHold(); return; }
            if (!HasActiveMap || player == null || player.IsDead) { CancelTraversal(); return; }
            if (isTraversing) { UpdateTraversal(); return; }
            HandleTraversalInput();
        }

        public void SetTerrainContext(IPlayerTerrainQueries context)
        {
            if (ReferenceEquals(terrain, context)) return;
            terrain = context;
            CancelTraversal();
        }

        public void ClearTerrainContext(IPlayerTerrainQueries context)
        {
            if (ReferenceEquals(terrain, context)) SetTerrainContext(null);
        }

        public bool CanMove(Vector3 current, Vector3 target)
        {
            return terrain != null ? terrain.CanPlayerMove(current, target, terrainHalfExtents)
                : PlayerWorldBindings.CanTraverse(current, target, terrainHalfExtents);
        }

        private void BindMapIfNeeded()
        {
            terrain?.TryBindPlayer(player, spawn => {
                player.UnlockY();
                baseWorldY = transform.position.y;
                spawn.y = baseWorldY;
                player.SpawnAt(spawn);
            });
        }

        private void HandleTraversalInput()
        {
            if (player.MovementExecutions.BlocksOrdinaryInput || !InputManager.Instance.Gameplay.TraversalHeld)
            {
                ResetHold();
                return;
            }

            Vector3 facing = player.GetLogicalFacingDirection();
            bool lavaJump = false;
            bool found = terrain.TryGetClimbDestination(
                transform.position, facing, out Vector3 destination);
            if (!found)
            {
                found = terrain.TryGetLavaJumpDestination(
                    transform.position, facing, out destination);
                lavaJump = found;
            }
            if (!found || !terrain.TryGetClearTraversalLanding(destination, facing,
                terrainHalfExtents, out destination)) { ResetHold(); return; }

            if (!hasPendingDestination || pendingLavaJump != lavaJump ||
                (pendingDestination - destination).sqrMagnitude > 0.001f)
            {
                pendingDestination = destination;
                pendingLavaJump = lavaJump;
                hasPendingDestination = true;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;
            if (holdTimer < holdDuration) return;
            StartTraversal(pendingDestination, pendingLavaJump);
        }

        private void StartTraversal(Vector3 destination, bool lavaJump)
        {
            if (!terrain.CanStartTraversal(transform.position, destination, terrainHalfExtents))
            {
                ResetHold();
                return;
            }

            if (!InputManager.Instance.Gameplay.CanReadGameplay || player.IsDead || player.IsBossDisplaced) { ResetHold(); return; }
            var execution = player.MovementExecutions.TryAcquire(this);
            if (execution == null) { ResetHold(); return; }
            traversalExecution = execution;

            try
            {
                traversalStart = transform.position;
                int targetLevel = terrain.GetHeightLevel(destination);
                destination.y = baseWorldY + targetLevel * heightStep;
                traversalDestination = destination;
                traversalDuration = lavaJump ? lavaJumpDuration : climbMoveDuration;
                pendingLavaJump = lavaJump;
                traversalTimer = 0f;
                isTraversing = true;
                ResetHold(false);
            }
            catch { CancelTraversal(); throw; }
        }

        private void UpdateTraversal()
        {
            if (traversalExecution == null || !traversalExecution.IsActive) { CancelTraversal(); return; }
            try
            {
                traversalTimer += Time.deltaTime;
                float t = Mathf.Clamp01(traversalTimer / traversalDuration);
                float smooth = t * t * (3f - 2f * t);
                Vector3 position = Vector3.Lerp(traversalStart, traversalDestination, smooth);
                if (pendingLavaJump) position.y += Mathf.Sin(t * Mathf.PI) * lavaJumpHeight;
                player.SpawnAt(position);

                if (t < 1f) return;
                player.SpawnAt(traversalDestination);
                CancelTraversal();
            }
            catch { CancelTraversal(); throw; }
        }

        private void OnDisable() => CancelTraversal();
        private void CancelTraversal()
        {
            traversalExecution?.Dispose(); traversalExecution = null;
            isTraversing = false; pendingLavaJump = false;
            ResetHold();
        }

        private void ResetHold(bool resetJumpKind = true)
        {
            holdTimer = 0f;
            hasPendingDestination = false;
            if (resetJumpKind) pendingLavaJump = false;
        }
    }
}
