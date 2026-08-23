using UnityEngine;
using UnityEngine.InputSystem;
using ProceduralMap;

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
        private MapGenerator map;
        private MapGenerator boundMap;
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

        public bool HasActiveMap => map && map.IsReady;
        public bool IsTraversing => isTraversing;
        public Vector2 TerrainHalfExtents => terrainHalfExtents;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            BindMapIfNeeded();
            if (!HasActiveMap) { ResetHold(); return; }
            if (isTraversing) { UpdateTraversal(); return; }
            HandleTraversalInput();
        }

        public bool CanMove(Vector3 current, Vector3 target)
        {
            if (!MidBossArenaController.CanPlayerTraverseArenaBoundary(current, target, terrainHalfExtents))
            {
                return false;
            }

            return !HasActiveMap || map.CanPlayerMoveWorld(current, target, terrainHalfExtents);
        }

        private void BindMapIfNeeded()
        {
            if (!map) map = FindFirstObjectByType<MapGenerator>();
            if (!map || !map.IsReady || boundMap == map) return;

            boundMap = map;
            player.UnlockY();
            Vector3 spawn = map.GetPlayerSpawnWorldPosition();
            baseWorldY = transform.position.y;
            spawn.y = baseWorldY;
            player.SpawnAt(spawn);
        }

        private void HandleTraversalInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.spaceKey.isPressed)
            {
                ResetHold();
                return;
            }

            Vector3 facing = player.GetLogicalFacingDirection();
            bool lavaJump = false;
            bool found = map.TryGetClimbDestinationWorld(
                transform.position, facing, out Vector3 destination, out _);
            if (!found)
            {
                found = map.TryGetLavaJumpDestinationWorld(
                    transform.position, facing, out destination);
                lavaJump = found;
            }
            if (!found) { ResetHold(); return; }

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
            if (!MidBossArenaController.CanPlayerTraverseArenaBoundary(
                    transform.position,
                    destination,
                    terrainHalfExtents))
            {
                ResetHold();
                return;
            }

            traversalStart = transform.position;
            int targetLevel = map.GetHeightLevelAtWorld(destination);
            destination.y = baseWorldY + targetLevel * heightStep;
            traversalDestination = destination;
            traversalDuration = lavaJump ? lavaJumpDuration : climbMoveDuration;
            pendingLavaJump = lavaJump;
            traversalTimer = 0f;
            isTraversing = true;
            ResetHold(false);
        }

        private void UpdateTraversal()
        {
            traversalTimer += Time.deltaTime;
            float t = Mathf.Clamp01(traversalTimer / traversalDuration);
            float smooth = t * t * (3f - 2f * t);
            Vector3 position = Vector3.Lerp(traversalStart, traversalDestination, smooth);
            if (pendingLavaJump) position.y += Mathf.Sin(t * Mathf.PI) * lavaJumpHeight;
            transform.position = position;

            if (t < 1f) return;
            transform.position = traversalDestination;
            isTraversing = false;
            pendingLavaJump = false;
        }

        private void ResetHold(bool resetJumpKind = true)
        {
            holdTimer = 0f;
            hasPendingDestination = false;
            if (resetJumpKind) pendingLavaJump = false;
        }
    }
}
