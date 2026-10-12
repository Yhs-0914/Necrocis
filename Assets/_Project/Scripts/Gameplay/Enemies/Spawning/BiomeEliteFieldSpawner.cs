using UnityEngine;

namespace Necrocis
{
    public sealed class BiomeEliteFieldSpawner : MonoBehaviour, IEnemySpawnOwner
    {
        private EnemySpawnRuleConfig rule;
        private string runId;
        private float activationDistance, releaseDistance;
        private bool defeated;
        private System.Action<string> recordLocalDefeat;
        private bool configured;
        private bool releasing;
        public BiomeElitePlacement Placement { get; private set; }
        public EnemyController ActiveEnemy { get; private set; }

        public void Configure(BiomeElitePlacement point, EnemySpawnRuleConfig source, BiomeEliteSpawnConfig settings, string currentRunId, System.Action<string> recordDefeat)
        {
            ReleaseEnemy();
            Placement = point;
            runId = currentRunId;
            recordLocalDefeat = recordDefeat;
            rule = source.CreateRuntimeCopy();
            // Combat numbers only come from MonsterDefinition. Spawn-rule kill flags have no role here.
            rule.isElite = true;
            rule.leashRadius = settings.leashDistance;
            activationDistance = settings.activationDistance;
            releaseDistance = settings.releaseDistance;
            defeated = SaveService.IsBiomeEliteDefeated(point.spawnId);
            configured = true;
        }

        public void Refresh(Vector3 playerPosition)
        {
            if (!configured || (runId != SaveService.ActiveRunId)) { ReleaseEnemy(); return; }
            Vector3 delta = playerPosition - transform.position;
            delta.y = 0;
            if (ActiveEnemy != null)
            {
                Vector3 enemyDelta = playerPosition - ActiveEnemy.transform.position;
                enemyDelta.y = 0;
                if (delta.sqrMagnitude > releaseDistance * releaseDistance && enemyDelta.sqrMagnitude > releaseDistance * releaseDistance)
                    ReleaseEnemy();
                return;
            }
            if (defeated) return; // A dead actor may finish its visual animation, but this point can never respawn it.
            if (delta.sqrMagnitude > activationDistance * activationDistance) return;
            EnemyController enemy = EnemyController.Acquire(transform, "BiomeElite_" + rule.name, EnemyController.GetPoolArchetypeId(rule));
            enemy.Configure(this, rule, transform.position, transform.position);
            ActiveEnemy = enemy;
            var lifetime = enemy.GetComponent<EnemyPatternLifetime>() ?? enemy.gameObject.AddComponent<EnemyPatternLifetime>();
            if (lifetime.Generation != enemy.SpawnGeneration) lifetime.Bind(enemy);
            enemy.Defeated += OnDefeated;
        }

        private void OnDefeated(EnemyController enemy)
        {
            if (enemy != ActiveEnemy || defeated) { enemy.SuppressExperienceReward = true; return; }
            defeated = true;
            recordLocalDefeat?.Invoke(Placement.spawnId);
            enemy.GetComponent<EnemyPatternLifetime>()?.Cancel();
            if (runId == null) return; // Unsaved development session: this placement still dies once in memory.
            if (!SaveService.TryRecordBiomeEliteDefeat(runId, Placement.spawnId, out bool fresh, out string error))
            {
                enemy.SuppressExperienceReward = true;
                Debug.LogError("[BiomeElite] 처치 저장 실패: " + error);
            }
            else if (!fresh) enemy.SuppressExperienceReward = true;
        }

        public void NotifyEnemyReleased(EnemyController enemy)
        {
            if (enemy != ActiveEnemy) return;
            enemy.Defeated -= OnDefeated;
            ActiveEnemy = null;
        }

        public void ReleaseEnemy()
        {
            if (releasing || ActiveEnemy == null) return;
            releasing = true;
            EnemyController enemy = ActiveEnemy;
            enemy.Defeated -= OnDefeated;
            ActiveEnemy = null;
            enemy.ReleaseToPool();
            releasing = false;
        }

        private void OnDisable() => ReleaseEnemy();
        private void OnDestroy() => ReleaseEnemy();
    }
}
