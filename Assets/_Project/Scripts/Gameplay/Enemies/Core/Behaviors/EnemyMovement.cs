using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {

        public bool IsPlayerInChaseRange()
        {
            if (playerTransform == null) return false;
            if (!ignoreMidBossArenaRestriction && MidBossArenaController.IsPlayerInsideLockedArena(playerTransform.position))
            {
                return false;
            }

            float distToPlayer = GetPlanarDistance(GetCurrentPosition(), playerTransform.position);
            if (distToPlayer > config.chaseRadius) return false;

            // leash 안에 있는 플레이어만 추격
            float playerToAnchor = GetPlanarDistance(playerTransform.position, anchorPosition);
            return playerToAnchor <= config.leashRadius;
        }


        public bool IsPlayerInAttackRange()
        {
            if (playerTransform == null) return false;
            if (!ignoreMidBossArenaRestriction && MidBossArenaController.IsPlayerInsideLockedArena(playerTransform.position))
            {
                return false;
            }
            float dist = GetPlanarDistance(GetCurrentPosition(), playerTransform.position);
            float effectiveAttackRange = config.isRanged
                ? config.attackRange
                : GetEffectiveMeleeAttackRange(PlayerController.Instance);
            return dist <= effectiveAttackRange;
        }


        public bool IsOutOfLeash()
        {
            return GetPlanarDistance(GetCurrentPosition(), anchorPosition) > config.leashRadius;
        }


        public bool IsIdleTimerExpired(float deltaTime)
        {
            idleTimer -= deltaTime;
            return idleTimer <= 0f;
        }


        public void ResetIdleTimer()
        {
            float min = Mathf.Min(config.idleDelayRange.x, config.idleDelayRange.y);
            float max = Mathf.Max(config.idleDelayRange.x, config.idleDelayRange.y);
            idleTimer = Random.Range(min, max);
        }


        public void PickWanderDestination()
        {
            if (TryPickWanderDestination(out Vector3 wanderDest))
            {
                SetDestination(wanderDest);
            }
        }


        public void SetChaseDestination()
        {
            if (playerTransform == null) return;
            Vector3 chase = playerTransform.position;
            chase.y = GetCurrentPosition().y;
            SetDestination(chase);
        }


        public void SetReturnDestination()
        {
            SetDestination(anchorPosition);
        }

        public bool MoveTowardDestination(float deltaTime)
        {
            if (statusEffectController != null && statusEffectController.IsStunned)
            {
                if (usingMoveAnimation)
                {
                    SetIdleAnimation();
                }
                return false;
            }

            if (!hasDestination) return false;

            Vector3 currentPosition = GetCurrentPosition();
            Vector3 separation = GetSeparationVector(currentPosition);

            Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
            Vector3 flatDestination = new Vector3(destination.x, 0f, destination.z);
            Vector3 toDestination = flatDestination - flatCurrent;
            float distance = toDestination.magnitude;

            if (distance <= Mathf.Max(0.01f, config.stoppingDistance))
            {
                hasDestination = false;
                return false; // 도착
            }

            Vector3 moveDirection = toDestination.normalized;
            if (separation.sqrMagnitude > 0.0001f)
            {
                Vector3 combined = moveDirection + separation * config.separationStrength;
                if (combined.sqrMagnitude > 0.0001f)
                {
                    moveDirection = combined.normalized;
                }
            }

            Vector3 step = moveDirection * (stats != null ? stats.MoveSpeed : 0f) * deltaTime;
            if (step.sqrMagnitude > toDestination.sqrMagnitude)
            {
                step = toDestination;
            }

            bool moved = TryMove(currentPosition, step);
            if (!moved)
            {
                hasDestination = false;
                return false;
            }

            if (spriteRenderer != null && Mathf.Abs(step.x) > 0.001f)
            {
                spriteRenderer.flipX = step.x < 0f;
            }

            // 이동 애니메이션 전환
            if (!usingMoveAnimation)
            {
                SetMoveAnimation();
            }

            return true; // 아직 이동 중
        }

        public void ApplyKnockback(Vector3 worldDirection, float distance)
        {
            if (IsDead || distance <= 0f) return;

            Vector3 planarDirection = new Vector3(worldDirection.x, 0f, worldDirection.z);
            if (planarDirection.sqrMagnitude <= 0.0001f) return;

            planarDirection.Normalize();
            Vector3 displacement = planarDirection * distance;
            Vector3 currentPosition = GetCurrentPosition();
            bool moved = TryMove(currentPosition, displacement);
            if (moved)
            {
                hasDestination = false;
            }
        }


        public void SetIgnoreMidBossArenaRestriction(bool ignore)
        {
            ignoreMidBossArenaRestriction = ignore;
        }


        public void SetAiSuppressed(bool suppressed)
        {
            if (aiSuppressed == suppressed)
            {
                return;
            }

            aiSuppressed = suppressed;
            hasDestination = false;
            attackAnimPlaying = false;

            if (suppressed)
            {
                SetIdleAnimation();
            }
        }


        public bool MoveByExternalPattern(Vector3 step)
        {
            if (IsDead || step.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            return TryMove(GetCurrentPosition(), step);
        }

        public void StartCharge()
        {
            if (playerTransform == null || config == null) return;

            Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return;

            chargeDirection = toPlayer.normalized;
            chargeElapsed = 0f;
            chargeCurrentSpeed = 0f;
            isCharging = true;

            // 방향에 따라 스프라이트 반전
            if (spriteRenderer != null)
                spriteRenderer.flipX = chargeDirection.x < 0f;
        }

        public bool UpdateCharge(float deltaTime)
        {
            if (!isCharging || config == null) return false;

            chargeElapsed += deltaTime;

            // 가속 단계
            float accelTime = config.chargeAccelTime;
            if (chargeElapsed < accelTime)
            {
                chargeCurrentSpeed = Mathf.Lerp(0f, config.chargeSpeed, chargeElapsed / accelTime);
            }
            else
            {
                chargeCurrentSpeed = config.chargeSpeed;
            }

            // 이동
            Vector3 step = chargeDirection * chargeCurrentSpeed * deltaTime;
            Vector3 currentPos = GetCurrentPosition();
            bool moved = TryMove(currentPos, step);

            if (!moved)
            {
                isCharging = false;
                return false;
            }

            // 플레이어를 지나쳤는지 체크 (플레이어와의 거리가 다시 멀어지기 시작하면 종료)
            if (playerTransform != null && chargeElapsed > accelTime)
            {
                Vector3 toPlayer = playerTransform.position - GetCurrentPosition();
                toPlayer.y = 0f;
                // 돌진 방향과 플레이어 방향이 반대면 지나친 것
                if (Vector3.Dot(chargeDirection, toPlayer.normalized) < 0f)
                {
                    isCharging = false;
                    return false;
                }
            }

            // 최대 돌진 시간 (1.5초)
            if (chargeElapsed > 1.5f)
            {
                isCharging = false;
                return false;
            }

            return true;
        }

        public void EndCharge()
        {
            isCharging = false;
            chargeCurrentSpeed = 0f;
            chargeCooldownTimer = 3f;
        }

        public void ApplyAggroBoost(float percentBoost)
        {
            if (config == null || hasAggroBoost) return;
            originalChaseRadius = config.chaseRadius;
            config.chaseRadius *= (1f + percentBoost);
            hasAggroBoost = true;
        }

        public void RemoveAggroBoost()
        {
            if (!hasAggroBoost || config == null) return;
            config.chaseRadius = originalChaseRadius;
            hasAggroBoost = false;
        }

        public void ForceChasePlayer()
        {
            if (IsDead) return;

            // 이동속도 3배 버프 (source로 "AggroDebris" 문자열 사용)
            if (stats != null)
            {
                stats.AddModifier(new CharacterStatModifier(
                    CharacterStatType.MoveSpeed,
                    2.0f,
                    CharacterStatModifierMode.PercentMultiply,
                    "AggroDebris"
                ));
            }

            // 공격 쿨타임 리셋 (즉시 공격 가능)
            attackTimer = 0f;

            ChangeState(EnemyChaseState.Instance);
        }

        public void RemoveAllAggroEffects()
        {
            RemoveAggroBoost();
            if (stats != null)
            {
                stats.RemoveModifiersFromSource("AggroDebris");
            }
        }


        private void SetDestination(Vector3 targetPosition)
        {
            destination = targetPosition;
            hasDestination = true;
        }


        private bool TryPickWanderDestination(out Vector3 wanderDestination)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                wanderDestination = anchorPosition;
                return true;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, config.wanderRadius);
                Vector3 candidate = anchorPosition + new Vector3(offset.x, 0f, offset.y);
                Vector2Int grid = biome.WorldToGrid(candidate);
                if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                    continue;

                candidate.y = biome.GetGroundHeight(candidate) + config.heightOffset;
                wanderDestination = candidate;
                return true;
            }

            wanderDestination = anchorPosition;
            wanderDestination.y = biome.GetGroundHeight(anchorPosition) + config.heightOffset;
            return true;
        }


        private bool TryMove(Vector3 currentPosition, Vector3 step)
        {
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                MoveToPosition(currentPosition + step);
                return true;
            }

            Vector3 targetPosition = currentPosition + step;
            if (biome.CanMove(currentPosition, targetPosition))
            {
                MoveToPosition(targetPosition);
                return true;
            }

            Vector3 moveX = new Vector3(step.x, 0f, 0f);
            Vector3 moveZ = new Vector3(0f, 0f, step.z);

            if (Mathf.Abs(step.x) >= Mathf.Abs(step.z))
            {
                if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveX))
                {
                    MoveToPosition(currentPosition + moveX);
                    return true;
                }
                if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveZ))
                {
                    MoveToPosition(currentPosition + moveZ);
                    return true;
                }
            }
            else
            {
                if (moveZ.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveZ))
                {
                    MoveToPosition(currentPosition + moveZ);
                    return true;
                }
                if (moveX.sqrMagnitude > 0f && biome.CanMove(currentPosition, currentPosition + moveX))
                {
                    MoveToPosition(currentPosition + moveX);
                    return true;
                }
            }

            return false;
        }

        private Vector3 GetSeparationVector(Vector3 currentPosition)
        {
            if (config == null || config.separationDistance <= 0f)
                return Vector3.zero;

            float maxDistanceSq = config.separationDistance * config.separationDistance;
            Vector3 separation = Vector3.zero;
            Vector2Int centerCell = ToSpatialCell(currentPosition);
            int searchRadius = Mathf.Max(1, Mathf.CeilToInt(config.separationDistance / SpatialHashCellSize));

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    if (!ActiveEnemyCells.TryGetValue(cell, out List<EnemyController> enemiesInCell))
                    {
                        continue;
                    }

                    for (int i = 0; i < enemiesInCell.Count; i++)
                    {
                        EnemyController other = enemiesInCell[i];
                        if (other == null || other == this || other.config == null) continue;

                        Vector3 delta = currentPosition - other.GetCurrentPosition();
                        delta.y = 0f;
                        float distanceSq = delta.sqrMagnitude;
                        if (distanceSq <= 0.0001f || distanceSq > maxDistanceSq) continue;

                        float distance = Mathf.Sqrt(distanceSq);
                        float weight = 1f - (distance / config.separationDistance);
                        separation += delta.normalized * weight;
                    }
                }
            }

            return separation;
        }


        private Vector3 GetCurrentPosition()
        {
            return body != null ? body.position : transform.position;
        }


        private void MoveToPosition(Vector3 position)
        {
            if (body != null)
            {
                body.position = position;
            }
            else
            {
                transform.position = position;
            }

            RegisterOrUpdateSpatialCell(position);
        }


        private void SetPosition(Vector3 position)
        {
            if (body != null)
            {
                body.position = position;
            }
            else
            {
                transform.position = position;
            }

            RegisterOrUpdateSpatialCell(position);
        }


        private static Vector2Int ToSpatialCell(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / SpatialHashCellSize),
                Mathf.FloorToInt(position.z / SpatialHashCellSize));
        }


        private void RegisterOrUpdateSpatialCell(Vector3 position)
        {
            Vector2Int newCell = ToSpatialCell(position);
            if (isRegisteredInSpatialHash && newCell == currentSpatialCell)
            {
                return;
            }

            if (isRegisteredInSpatialHash)
            {
                UnregisterSpatialCell();
            }

            if (!ActiveEnemyCells.TryGetValue(newCell, out List<EnemyController> list))
            {
                list = new List<EnemyController>();
                ActiveEnemyCells.Add(newCell, list);
            }

            list.Add(this);
            currentSpatialCell = newCell;
            isRegisteredInSpatialHash = true;
        }


        private void UnregisterSpatialCell()
        {
            if (!isRegisteredInSpatialHash)
            {
                return;
            }

            if (ActiveEnemyCells.TryGetValue(currentSpatialCell, out List<EnemyController> list))
            {
                list.Remove(this);
                if (list.Count == 0)
                {
                    ActiveEnemyCells.Remove(currentSpatialCell);
                }
            }

            isRegisteredInSpatialHash = false;
        }


        private static float GetPlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }


        private float GetEffectiveMeleeAttackRange(PlayerController player)
        {
            if (config == null)
            {
                return 0f;
            }

            float bodyContactRange = GetMeleeContactRange(config.colliderSize, player);
            Vector3 activeAttackSize = config.expandColliderOnAttack
                ? config.attackColliderSize
                : config.colliderSize;
            float attackContactRange = GetMeleeContactRange(activeAttackSize, player);
            float minRange = Mathf.Max(0.25f, bodyContactRange);
            float maxRange = Mathf.Max(minRange, attackContactRange);

            if (config.attackRange <= 0f)
            {
                return maxRange;
            }

            return Mathf.Clamp(config.attackRange, minRange, maxRange);
        }


        private float GetMeleeContactRange(Vector3 localSize, PlayerController player)
        {
            Vector3 scaledSize = Vector3.Scale(localSize, Abs(transform.lossyScale));
            float enemyReach = Mathf.Max(scaledSize.x, scaledSize.z) * 0.5f;
            float playerRadius = GetPlayerPlanarRadius(player);
            return enemyReach + playerRadius + 0.2f;
        }


        private static float GetPlayerPlanarRadius(PlayerController player)
        {
            if (player == null)
            {
                return 0.45f;
            }

            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                playerCollider = player.GetComponentInChildren<Collider>();
            }

            if (playerCollider == null || !playerCollider.enabled)
            {
                return 0.45f;
            }

            Bounds bounds = playerCollider.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.z);
        }

    }
}
