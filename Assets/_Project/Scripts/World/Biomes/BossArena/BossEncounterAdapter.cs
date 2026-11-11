using System;
using UnityEngine;

namespace Necrocis
{
    // Specific pattern components and the lung's two bodies are composed here.
    internal sealed class BossEncounterAdapter : IBossEncounter
    {
        private readonly EnemyController mainBoss;
        private IntestineBossPattern activeIntestinePattern;
        private LiverBossPattern activeLiverPattern;
        private StomachBossPattern activeStomachPattern;
        private LungBossPattern activeLungPattern;

        private BossEncounterAdapter(EnemyController boss) { mainBoss = boss; }
        public static IBossEncounter Create(MidBossPatternType patternType, EnemyController boss,
            Vector3 position, Transform parent, MidBossDefinition definition, Action<EnemyController> registerContact)
        {
            var encounter = new BossEncounterAdapter(boss);
            encounter.Initialize(patternType, boss, position, parent, definition, registerContact);
            return encounter;
        }
        public bool HasPattern => activeIntestinePattern != null || activeLiverPattern != null
            || activeStomachPattern != null || activeLungPattern != null;
        public bool IsGroup => activeLungPattern != null;
        public bool IsDefeated => activeLungPattern != null ? activeLungPattern.IsEncounterDefeated
            : mainBoss == null || mainBoss.IsDead || !mainBoss.gameObject.activeInHierarchy;
        public void SetActive(bool active)
        {
            activeIntestinePattern?.SetEncounterActive(active);
            activeLiverPattern?.SetEncounterActive(active);
            activeStomachPattern?.SetEncounterActive(active);
            activeLungPattern?.SetEncounterActive(active);
        }
        public void ForEachActor(Action<EnemyController> action)
        {
            if (activeLungPattern != null) activeLungPattern.ForEachEncounterBoss(action);
            else if (mainBoss != null) action(mainBoss);
        }
        public void Recenter() => activeLungPattern?.RecenterEncounter();
        public void Dispose() => activeLungPattern?.DisposeEncounter();

        private void Initialize(MidBossPatternType patternType, EnemyController boss, Vector3 bossSpawnPosition,
            Transform parent, MidBossDefinition definition, Action<EnemyController> registerContact)
        {
            IntestineBossPattern intestinePattern = boss.GetComponent<IntestineBossPattern>();
            LiverBossPattern liverPattern = boss.GetComponent<LiverBossPattern>();
            StomachBossPattern stomachPattern = boss.GetComponent<StomachBossPattern>();
            LungBossPattern lungPattern = parent.GetComponent<LungBossPattern>();
            activeIntestinePattern = null;
            activeLiverPattern = null;
            activeStomachPattern = null;
            activeLungPattern = null;

            if (patternType == MidBossPatternType.Intestine)
            {
                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (intestinePattern == null)
                {
                    intestinePattern = boss.gameObject.AddComponent<IntestineBossPattern>();
                }

                intestinePattern.Initialize(boss, bossSpawnPosition, parent, definition?.intestinePattern);
                intestinePattern.SetEncounterActive(false);
                activeIntestinePattern = intestinePattern;
                return;
            }

            if (patternType == MidBossPatternType.Liver)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (liverPattern == null)
                {
                    liverPattern = boss.gameObject.AddComponent<LiverBossPattern>();
                }

                liverPattern.Initialize(boss, bossSpawnPosition, parent, definition?.liverPattern);
                liverPattern.SetEncounterActive(false);
                activeLiverPattern = liverPattern;
                return;
            }

            if (patternType == MidBossPatternType.Stomach)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (lungPattern != null)
                {
                    lungPattern.enabled = false;
                }

                if (stomachPattern == null)
                {
                    stomachPattern = boss.gameObject.AddComponent<StomachBossPattern>();
                }

                stomachPattern.Initialize(boss, bossSpawnPosition, parent, definition?.stomachPattern);
                stomachPattern.SetEncounterActive(false);
                activeStomachPattern = stomachPattern;
                return;
            }

            if (patternType == MidBossPatternType.Lung)
            {
                if (intestinePattern != null)
                {
                    intestinePattern.enabled = false;
                }

                if (liverPattern != null)
                {
                    liverPattern.enabled = false;
                }

                if (stomachPattern != null)
                {
                    stomachPattern.enabled = false;
                }

                if (lungPattern == null)
                {
                    lungPattern = parent.gameObject.AddComponent<LungBossPattern>();
                }

                activeLungPattern = lungPattern;
                lungPattern.Initialize(boss, bossSpawnPosition, parent, definition?.lungPattern);
                lungPattern.SetEncounterActive(false);
                lungPattern.ForEachEncounterBoss(registerContact);
                return;
            }

            if (intestinePattern != null)
            {
                intestinePattern.enabled = false;
            }

            if (liverPattern != null)
            {
                liverPattern.enabled = false;
            }

            if (stomachPattern != null)
            {
                stomachPattern.enabled = false;
            }

            if (lungPattern != null)
            {
                lungPattern.enabled = false;
            }

            boss.SetAiSuppressed(true);
        }

    }
}
