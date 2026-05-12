using System.Collections.Generic;
using UnityEngine;

namespace Necrocis
{
    public partial class EnemyController
    {
        public void HandleEliteDeath()
        {
            if (config == null || !config.isElite) return;

            Debug.Log($"[EliteDeath] {config.name} 사망 처리 시작 - splitsOnDeath={config.splitsOnDeath}, leavesDebris={config.leavesDebrisOnDeath}");

            // 육아종: 분열
            if (config.splitsOnDeath)
            {
                SpawnSplitEnemies();
            }

            // 항체: 잔해 생성
            if (config.leavesDebrisOnDeath)
            {
                SpawnAggroDebris();
            }
        }


        private void SpawnSplitEnemies()
        {
            Debug.Log($"[EliteDeath] SpawnSplitEnemies 호출 - splitEnemyName='{config.splitEnemyName}'");

            if (string.IsNullOrEmpty(config.splitEnemyName))
            {
                Debug.LogWarning("[EliteDeath] splitEnemyName이 비어있음! 분열 불가.");
                return;
            }

            EnemySpawnRuleConfig splitConfig = FindEnemyConfigByName(config.splitEnemyName);
            if (splitConfig == null)
            {
                Debug.LogWarning($"[EliteDeath] '{config.splitEnemyName}' 설정을 찾을 수 없음! BiomeConfig에 해당 적이 없음.");
                return;
            }

            Debug.Log($"[EliteDeath] 분열 설정 찾음: {splitConfig.name}, splitCount={config.splitCount}");

            Vector3 deathPos = GetCurrentPosition();

            // VoidShield 이펙트 로드 → 이펙트 재생 후 분열
            Sprite[] vfxSprites = LoadVoidShieldSprites();
            if (vfxSprites != null && vfxSprites.Length > 0)
            {
                Debug.Log($"[EliteDeath] VoidShield 이펙트 재생 시작 ({vfxSprites.Length}프레임)");
                SpawnSplitVfxThenSpawn(deathPos, splitConfig, vfxSprites);
            }
            else
            {
                Debug.Log("[EliteDeath] VoidShield 스프라이트 없음, 즉시 분열");
                DoSpawnSplitEnemies(deathPos, splitConfig);
            }
        }

        private void SpawnSplitVfxThenSpawn(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, Sprite[] vfxSprites)
        {
            // VFX 오브젝트 생성
            GameObject vfxObj = new GameObject($"SplitVFX_{config.name}");
            vfxObj.transform.position = deathPos + new Vector3(0f, 0.8f, 0f);

            SpriteRenderer vfxRenderer = vfxObj.AddComponent<SpriteRenderer>();
            vfxRenderer.sprite = vfxSprites[0];
            vfxRenderer.sortingOrder = config.sortingOrder + 200;
            vfxRenderer.color = new Color(0.5f, 0.2f, 1f, 0.9f);

            Billboard vfxBb = vfxObj.AddComponent<Billboard>();
            vfxBb.enabled = true;

            vfxObj.transform.localScale = Vector3.one * config.splitVfxScale;

            SpriteFrameAnimator vfxAnim = vfxObj.AddComponent<SpriteFrameAnimator>();
            vfxAnim.enabled = true;

            // 캡처용 로컬 변수
            int splitCount = config.splitCount;
            Transform spawnParent = transform.parent != null ? transform.parent : transform;

            // 이펙트 재생 → 완료 후 분열
            vfxAnim.PlayOneShot(vfxSprites, config.splitVfxSpeed, () =>
            {
                // 이펙트 종료 후 분열 적 소환
                DoSpawnSplitEnemies(deathPos, splitConfig, splitCount, spawnParent);

                // VFX 페이드 아웃 후 제거
                SplitVfxFadeOut fadeOut = vfxObj.AddComponent<SplitVfxFadeOut>();
                fadeOut.Init(vfxRenderer, 0.3f);
            });
        }


        private Sprite[] LoadVoidShieldSprites()
        {
            // 1) config에 직접 할당된 스프라이트가 있으면 사용
            if (config.splitVfxSprites != null && config.splitVfxSprites.Length > 0)
                return config.splitVfxSprites;

            // 2) 캐시된 스프라이트가 있으면 재사용
            if (cachedVoidShieldSprites != null && cachedVoidShieldSprites.Length > 0)
                return cachedVoidShieldSprites;

            // 3) Resources에서 VoidShield 스프라이트 로드 시도
            Sprite[] loaded = Resources.LoadAll<Sprite>("VoidShield_Lite");
            if (loaded != null && loaded.Length > 0)
            {
                // 이름순 정렬 (VoidShield_0, 1, 2, ...)
                System.Array.Sort(loaded, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
                cachedVoidShieldSprites = loaded;
                return cachedVoidShieldSprites;
            }

            // 4) 프로시저럴 폴백: 간단한 원형 스프라이트 6프레임 생성
            cachedVoidShieldSprites = GenerateVoidShieldSprites(6, 64);
            return cachedVoidShieldSprites;
        }

        private static Sprite[] GenerateVoidShieldSprites(int frameCount, int size)
        {
            Sprite[] sprites = new Sprite[frameCount];
            for (int f = 0; f < frameCount; f++)
            {
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                float phase = (float)f / frameCount;
                float center = size * 0.5f;
                float maxRadius = size * 0.45f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;

                        // 쉴드 링: 중심 비우고 바깥쪽 링
                        float innerRadius = 0.5f + phase * 0.3f;
                        float outerRadius = 0.9f + phase * 0.1f;
                        float ring = 1f - Mathf.Clamp01(Mathf.Abs(dist - (innerRadius + outerRadius) * 0.5f) / ((outerRadius - innerRadius) * 0.5f));
                        float alpha = ring * (1f - phase * 0.5f);

                        // 보라색 그라데이션
                        float r = Mathf.Lerp(0.4f, 0.7f, phase);
                        float g = Mathf.Lerp(0.1f, 0.3f, phase);
                        float b = Mathf.Lerp(0.8f, 1f, phase);

                        tex.SetPixel(x, y, new Color(r, g, b, alpha));
                    }
                }

                tex.Apply();
                sprites[f] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
                sprites[f].name = $"VoidShield_Gen_{f}";
            }
            return sprites;
        }


        private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig)
        {
            Transform spawnParent = transform.parent != null ? transform.parent : transform;
            DoSpawnSplitEnemies(deathPos, splitConfig, config.splitCount, spawnParent);
        }


        private void DoSpawnSplitEnemies(Vector3 deathPos, EnemySpawnRuleConfig splitConfig, int count, Transform spawnParent)
        {
            Debug.Log($"[EliteDeath] DoSpawnSplitEnemies: count={count}, deathPos={deathPos}, parent={spawnParent?.name}");

            BiomeManager biome = BiomeManager.Active;
            int poolId = GetPoolArchetypeId(splitConfig);

            for (int i = 0; i < count; i++)
            {
                // 사망 위치 주변에 분산 배치
                Vector2 offset = Random.insideUnitCircle * 1.5f;
                Vector3 spawnPos = deathPos + new Vector3(offset.x, 0f, offset.y);

                if (biome != null)
                {
                    Vector2Int grid = biome.WorldToGrid(spawnPos);
                    if (!biome.IsValidPosition(grid.x, grid.y) || !biome.IsWalkable(grid.x, grid.y))
                    {
                        spawnPos = deathPos;
                    }
                    spawnPos.y = biome.GetGroundHeight(spawnPos) + splitConfig.heightOffset;
                }

                // 직접 적 생성 (스포너 없이 즉시)
                EnemyController split = Acquire(spawnParent, $"{splitConfig.name}_Split_{i}", poolId);
                split.Configure(null, splitConfig, spawnPos, spawnPos);
                Debug.Log($"[EliteDeath] 분열 적 #{i} 생성 완료: {split.gameObject.name} at {spawnPos}");
            }
        }


        private void SpawnAggroDebris()
        {
            Vector3 pos = GetCurrentPosition();
            pos.y += 0.5f;

            GameObject debrisObj = new GameObject($"AggroDebris_{config.name}");
            debrisObj.transform.position = pos;

            AggroDebris debris = debrisObj.AddComponent<AggroDebris>();

            // 잔해 스프라이트: 사망 스프라이트의 마지막 프레임
            Sprite debrisSprite = null;
            if (config.deathSprites != null && config.deathSprites.Length > 0)
                debrisSprite = config.deathSprites[config.deathSprites.Length - 1];
            else if (config.idleSprites != null && config.idleSprites.Length > 0)
                debrisSprite = config.idleSprites[0];

            debris.Configure(
                config.debrisDuration,
                config.debrisAggroRadius,
                debrisSprite,
                config.sortingOrder,
                config.debrisVfxSprites,
                config.debrisVfxScale,
                config.debrisVfxSpeed
            );
        }


        private EnemySpawnRuleConfig FindEnemyConfigByName(string enemyName)
        {
            // ConfigurableBiomeManager에서 현재 바이옴의 적 설정 검색
            BiomeManager biome = BiomeManager.Active;
            if (biome == null)
            {
                Debug.LogWarning("[EliteDeath] FindEnemyConfigByName: BiomeManager.Active == null");
                return null;
            }

            ConfigurableBiomeManager configBiome = biome as ConfigurableBiomeManager;
            if (configBiome == null)
            {
                Debug.LogWarning($"[EliteDeath] FindEnemyConfigByName: ConfigurableBiomeManager 캐스트 실패, 타입={biome.GetType().Name}");
                return null;
            }

            BiomeConfig biomeConfig = configBiome.GetBiomeConfig();
            if (biomeConfig == null)
            {
                Debug.LogWarning("[EliteDeath] FindEnemyConfigByName: BiomeConfig == null");
                return null;
            }

            IReadOnlyList<EnemySpawnRuleConfig> enemySpawnRules = biomeConfig.GetEnemySpawnRules();
            foreach (EnemySpawnRuleConfig rule in enemySpawnRules)
            {
                if (rule != null && rule.name == enemyName)
                    return rule;
            }
            Debug.LogWarning($"[EliteDeath] FindEnemyConfigByName: '{enemyName}'을 찾을 수 없음! 등록된 적: {enemySpawnRules.Count}개");
            return null;
        }

    }
}
