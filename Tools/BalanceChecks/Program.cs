using System.Text.Json;
using Necrocis;

var json = new JsonSerializerOptions { IncludeFields = true };
using var inputs = JsonDocument.Parse(File.ReadAllText(args[0]));
int assertions = 0;
void Require(bool condition, string message) { if (!condition) throw new Exception(message); assertions++; }
void Equal(float expected, float actual, string message) => Require(MathF.Abs(expected - actual) < .001f, $"{message}: {actual} expected {expected}");
T Read<T>(string key) => inputs.RootElement.GetProperty(key).Deserialize<T>(json);
var growth = Read<LevelProgressionConfig>("growth");
var stages = Read<MonsterProgressionProfile>("stages");
var normal = Read<DifficultyBalanceProfile>("normal");
var hard = Read<DifficultyBalanceProfile>("hard");
var definitions = Read<List<MonsterDefinition>>("definitions");
DifficultyBalanceService.Progression = growth;
DifficultyBalanceService.ActiveProfile = normal;

foreach (var (type, expected, mode) in new[]
{
    (CharacterStatType.AttackPower, 1f, CharacterStatModifierMode.Flat),
    (CharacterStatType.AttackSpeed, 20f, CharacterStatModifierMode.PercentAdd),
    (CharacterStatType.AttackRange, 10f, CharacterStatModifierMode.PercentAdd),
    (CharacterStatType.Magic, 25f, CharacterStatModifierMode.Flat),
    (CharacterStatType.MoveSpeed, 5f, CharacterStatModifierMode.PercentAdd),
    (CharacterStatType.MaxHealth, 2f, CharacterStatModifierMode.Flat)
})
{
    Require(growth.TryGetLevelUpStatValue(type, out var value), "level-up value exists " + type);
    Equal(expected, value.value, "applied level-up amount " + type);
    Require(value.mode == mode, "level-up modifier mode " + type);
    Equal(mode == CharacterStatModifierMode.PercentAdd ? expected / 100f : expected,
        growth.GetRuntimeModifierValue(value), "percent conversion " + type);
    var fallback = new LevelProgressionConfig();
    Require(fallback.TryGetLevelUpStatValue(type, out var defaultValue), "fallback growth exists " + type);
    Equal(expected, defaultValue.value, "fallback matches asset " + type);
}
Equal(1f, LevelUpStatCatalog.GetStatEffect(LevelUpStatChoice.AttackPowerUp).flatStats[CharacterStatType.AttackPower], "legacy choice uses attack +1");
var alternateGrowth = new LevelProgressionConfig();
alternateGrowth.levelUpStatValues.Single(value => value.statType == CharacterStatType.AttackPower).value = 2f;
DifficultyBalanceService.Progression = alternateGrowth;
Equal(2f, LevelUpStatCatalog.GetStatEffect(LevelUpStatChoice.AttackPowerUp).flatStats[CharacterStatType.AttackPower], "choice follows active profile after switching");
DifficultyBalanceService.Progression = growth;
Equal(1f, LevelUpStatCatalog.GetStatEffect(LevelUpStatChoice.AttackPowerUp).flatStats[CharacterStatType.AttackPower], "switch back cannot retain old cached growth");

var stats = new CharacterStats();
stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 6.9f));
for (int i = 0; i < 3; i++) stats.ApplyDamage(.5f);
Equal(5.4f, stats.CurrentHealth, "fractional multi-hit damage");
stats.AddModifier(CharacterStatType.MoveSpeed, 1f);
Equal(5.4f, stats.CurrentHealth, "HP survives stat refresh");
stats.SetCurrentHealth(.25f);
Equal(.25f, stats.CurrentHealth, "fractional enemy restore");
stats.RestoreHealth(.5f);
Equal(.75f, stats.CurrentHealth, "fractional enemy healing");
stats.UseWholeHealthUnits();
stats.ConfigureBaseStats(new CharacterStatValue(CharacterStatType.MaxHealth, 10));
stats.ApplyDamage(.75f);
Equal(10, stats.CurrentHealth, "player whole-heart units");
float remainder = stats.DamageRemainder;
stats.SetCurrentHealth(stats.CurrentHealth);
stats.RestoreHealthRemainders(remainder, 0);
for (int i = 0; i < 3; i++) stats.ApplyDamage(.75f);
Equal(7, stats.CurrentHealth, "four 25% reduced hits after remainder restore");
stats.ResetHealthToMax();
stats.ApplyDamage(1);
for (int i = 0; i < 4; i++) stats.RestoreHealth(.25f);
Equal(10, stats.CurrentHealth, "four fractional heals");
stats.ApplyDamage(float.NaN);
stats.ApplyDamage(float.PositiveInfinity);
Equal(10, stats.CurrentHealth, "non-finite damage rejected");
Equal(8990, Enumerable.Range(1, 29).Sum(growth.GetRequiredExpForCurrentLevel), "Lv30 cumulative experience");
LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
LevelUpManager.AddResolvedEnemyKillExp(8990);
Equal(5, LevelUpManager.GetCurrentLevel(), "stop at level five job selection");
Equal(8750, LevelUpManager.GetCurrentExp(), "bank large reward");
LevelUpManager.AddResolvedEnemyKillExp(40);
Equal(8790, LevelUpManager.GetCurrentExp(), "bank rewards while waiting");
var saved = new PlayerProgressSaveData();
LevelUpManager.CaptureProgress(saved);
LevelUpManager.RestoreProgress(saved);
Equal(8790, LevelUpManager.GetCurrentExp(), "banked XP survives load");
LevelUpManager.SetJob(JobType.Mage);
Equal(30, LevelUpManager.GetCurrentLevel(), "resume banked growth");
LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 7, experience = 200, job = JobType.None });
Equal(200, LevelUpManager.GetCurrentExp(), "legacy unselected job retains banked XP after a selected job");
Require(!LevelUpManager.IsSkillUnlocked(1), "job required before E");
LevelUpManager.SetJob(JobType.Warrior);
Require(LevelUpManager.IsSkillUnlocked(1), "legacy player gains E after choosing job");
LevelUpManager.RestoreProgress(new PlayerProgressSaveData { level = 1, job = JobType.Mage });
Require(!LevelUpManager.IsSkillUnlocked(1) && !LevelUpManager.IsSkillUnlocked(2), "skills locked before level five even with a saved job");
LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
LevelUpManager.AddResolvedEnemyKillExp(240);
Equal(5, LevelUpManager.GetCurrentLevel(), "job selection reached with 240 XP");
Require(!LevelUpManager.IsSkillUnlocked(1), "E waits for job selection at level five");
LevelUpManager.SetJob(JobType.Mage);
Require(LevelUpManager.IsSkillUnlocked(1) && !LevelUpManager.IsSkillUnlocked(2), "E unlocks with level five job selection");
LevelUpManager.AddResolvedEnemyKillExp(560);
Equal(9, LevelUpManager.GetCurrentLevel(), "level nine before R");
Require(!LevelUpManager.IsSkillUnlocked(2), "R remains locked at level nine");
LevelUpManager.AddResolvedEnemyKillExp(190);
Equal(10, LevelUpManager.GetCurrentLevel(), "R reached with cumulative 990 XP");
Require(LevelUpManager.IsSkillUnlocked(2), "R unlocked at level ten");
foreach (var (level, expectedMin, expectedMax) in new[]
{
    (0, 1, 2), (1, 1, 2), (5, 1, 2), (6, 2, 3), (10, 2, 3),
    (11, 3, 4), (15, 3, 4), (16, 4, 5), (20, 4, 5),
    (21, 5, 6), (25, 5, 6), (26, 6, 7), (30, 6, 7), (31, 6, 7)
})
{
    growth.GetNormalEnemySpawnCountRange(level, out int min, out int max);
    Require(min == expectedMin && max == expectedMax, "normal spawn population boundary at level " + level);
}
var spawnTuning = new LevelProgressionConfig
{
    normalEnemySpawnLevelInterval = 3, normalEnemySpawnMinCount = 2, normalEnemySpawnMaxCount = 4
};
spawnTuning.GetNormalEnemySpawnCountRange(4, out int tunedMin, out int tunedMax);
Require(tunedMin == 3 && tunedMax == 5, "custom spawn interval and population");
spawnTuning.normalEnemySpawnLevelInterval = 0;
spawnTuning.normalEnemySpawnMinCount = 0;
spawnTuning.normalEnemySpawnMaxCount = -1;
spawnTuning.GetNormalEnemySpawnCountRange(1, out tunedMin, out tunedMax);
Require(tunedMin == 1 && tunedMax == 1, "invalid spawn tuning cannot create zero enemies or divide by zero");
Require(stages.GetValidationError() == null, "valid progression profile");
Require(definitions.Count == 30, "30 monster definitions");
foreach (var definition in definitions)
    foreach (var profile in new[] { normal, hard })
        foreach (var stage in Enumerable.Range(0, 5))
            foreach (var phase in definition.statSets)
                Require(MonsterBalanceResolver.TryResolve(definition, phase.id, profile, stages, stage, out _, out var error), definition.monsterId + ": " + error);
int[] eliteRewards = { 100, 120, 125, 140, 160 }, bossRewards = { 240, 300, 300, 350, 400 };
for (int i = 0; i < 5; i++)
{
    var elite = definitions.Single(x => x.monsterId == "elite.i-01");
    var boss = definitions.Single(x => x.monsterId == "boss.intestine");
    MonsterBalanceResolver.TryResolve(elite, "Default", normal, stages, i, out var e, out _);
    MonsterBalanceResolver.TryResolve(boss, "Default", normal, stages, i, out var b, out _);
    Equal(eliteRewards[i], e.Experience, "elite reward " + i);
    Equal(bossRewards[i], b.Experience, "boss reward " + i);
    Equal(360, b.MaxHealth, "boss health independent of stage " + i);
}
// Old/custom profiles must not restore boss-clear HP scaling for any tier or phase.
var legacyHealthStages = new MonsterProgressionProfile();
foreach (var stage in legacyHealthStages.stages)
{
    stage.maxHealth = 1f + stage.stage * .4f;
    stage.bossHealth = 1f + stage.stage * .6f;
}
foreach (var profile in new[] { normal, hard })
foreach (var definition in definitions)
foreach (var phase in definition.statSets)
foreach (int stage in Enumerable.Range(0, 5))
{
    Require(MonsterBalanceResolver.TryResolve(definition, phase.id, profile, legacyHealthStages, stage, out var result, out _),
        "legacy progression resolves " + definition.monsterId);
    Equal(phase.maxHealth * MonsterBalanceResolver.GetTierBalance(profile, definition.tier).maxHealth,
        result.MaxHealth, "health independent of boss-clear stage " + definition.monsterId + "/" + phase.id + "/" + stage);
}
var noReward = definitions.Single(x => x.monsterId == "normal.intestine-parasite");
MonsterBalanceResolver.TryResolve(noReward, "Default", normal, stages, 4, out var parasite, out _);
Equal(0, parasite.Experience, "zero-reward summon preserved");
var player = new PlayerStats { AttackPower = 2.5f, AttackSpeed = 1.2f, Magic = 25, SkillCooldownReduction = 80 };
Equal(2.5f, PlayerCombatCalculator.GetBasicAttackDamage(player), "fractional attack");
Equal(20, PlayerCombatCalculator.GetSkillDamage(16, player), "magic scales skill damage");
Equal(6, PlayerCombatCalculator.GetSkillCooldown(10, player), "cooldown capped at 40%");
Equal(.625f, PlayerCombatCalculator.GetBasicAttackCooldown(.75f, player), "attack-speed scaling");
int[] rolls = new int[3];
for (int i = 0; i < 100000; i++) rolls[growth.RollGrowthMultiplier()]++;
Require(rolls[0] == 0 && Math.Abs(rolls[1] - 70000) < 1000 && Math.Abs(rolls[2] - 30000) < 1000, "growth gamble guarantees growth with 30 percent double growth");
var catalog = Read<string[]>("itemIds");
var starterCandidates = new HashSet<string>();
var starterSelections = new HashSet<string>();
for (int seed = 0; seed < 512; seed++)
{
    var starter = CampaignRewardRules.ChooseStarterOffers(catalog, Array.Empty<string>(), seed);
    Require(starter.Count == 3 && starter.Distinct().Count() == 3, "three distinct starter mutations");
    Require(starter.All(catalog.Contains), "starter mutations exist in catalog");
    Require(starter.SequenceEqual(CampaignRewardRules.ChooseStarterOffers(catalog, Array.Empty<string>(), seed)), "starter seed reproduces choices");
    Require(!CampaignRewardRules.ChooseStarterOffers(catalog, new[] { "double_core" }, seed).Contains("double_core"), "owned starter mutation excluded");
    var savedStarter = new WorldRewardSaveData { id = CampaignRewardRules.StarterRewardId, itemChoices = starter };
    var restoredStarter = JsonSerializer.Deserialize<WorldRewardSaveData>(JsonSerializer.Serialize(savedStarter, json), json);
    Require(starter.SequenceEqual(CampaignRewardRules.RefreshOffers(restoredStarter.itemChoices, catalog, Array.Empty<string>(), null, seed + 1)), "saved starter choices survive reload without rerolling");
    starterCandidates.UnionWith(starter);
    starterSelections.Add(string.Join(",", starter));
}
Require(starterCandidates.SetEquals(catalog), "every catalog item can appear as a starter choice");
Require(starterSelections.Count > 1, "new starter seeds produce different choices");
Require(CampaignRewardRules.ChooseStarterOffers(null, null, 0).Count == 0, "empty starter catalog");
Require(CampaignRewardRules.ChooseStarterOffers(new[] { "double_core", "DOUBLE_CORE", "triple_core", "", null }, null, 0).Count == 2, "small starter pool excludes duplicates and invalid IDs");
Require(CampaignRewardRules.ChooseStarterOffers(new[] { "double_core", "triple_core", "blood_drone" }, null, 0).Count == 3, "starter choices have no role quotas");
var owned = new[] { "double_core", "exoskeleton", "homing_cell" };
for (int seed = 0; seed < 250; seed++)
{
    var offers = CampaignRewardRules.ChooseOffers(catalog, owned, "triple_core", seed);
    Require(offers.Count == 3 && offers.Distinct().Count() == 3, "three distinct offers");
    Require(offers.All(id => !owned.Contains(id)), "owned items excluded");
    Require(offers.Select(CampaignRewardRules.Role).Distinct().Count() == 3, "attack / survival / utility coverage");
    Require(offers.Contains("triple_core"), "preferred available item retained");
    Require(offers.SequenceEqual(CampaignRewardRules.ChooseOffers(catalog, owned, "triple_core", seed)), "deterministic offers");
}
var lateOffers = CampaignRewardRules.ChooseOffers(catalog, catalog, null, 1);
Require(lateOffers.Count == 3 && lateOffers.All(PlayerStatItemCatalog.IsRepeatable), "owned catalog retains three repeatable stat rewards");
Require(CampaignRewardRules.ChooseOffers(null, null, null, 1).Count == 0, "missing reward pool");
Require(CampaignRewardRules.ChooseOffers(catalog, owned, "TRIPLE_CORE", 1).Contains("triple_core"), "case-insensitive item ID matching");
Require(CampaignRewardRules.ChooseOffers(new[] { "double_core" }, Array.Empty<string>(), null, 1).Count == 1, "small pool has no duplicates");
foreach (int level in new[] { 0, 1, 5, 6, 10, 11, 15, 16, 20, 21, 25, 26, 30, 100 })
{
    int band = (Math.Clamp(level, 1, 30) - 1) / 5;
    Equal(1f, growth.GetEnemyHealthMultiplier(level, MonsterTier.Normal), "normal health has no level percentage " + level);
    Equal(band * 2f, growth.GetEnemyHealthBonus(level, MonsterTier.Normal), "normal flat health band " + level);
    Equal(0f, growth.GetEnemyHealthBonus(level, MonsterTier.Elite), "elite excluded from flat health " + level);
    Equal(0f, growth.GetEnemyHealthBonus(level, MonsterTier.Boss), "boss excluded from flat health " + level);
    Equal(1f + band * .12f, growth.GetEnemyHealthMultiplier(level, MonsterTier.Elite), "elite health band " + level);
    Equal(1f, growth.GetEnemyHealthMultiplier(level, MonsterTier.Boss), "boss excluded from level health " + level);
}
var healthTuning = new LevelProgressionConfig { enemyHealthLevelInterval = 3, normalEnemyHealthBonusPerBand = 4 };
Equal(4f, healthTuning.GetEnemyHealthBonus(4, MonsterTier.Normal), "custom flat health interval and amount");
healthTuning.enemyHealthLevelInterval = 0;
healthTuning.normalEnemyHealthBonusPerBand = -2;
Equal(0f, healthTuning.GetEnemyHealthBonus(6, MonsterTier.Normal), "invalid health tuning cannot reduce health or divide by zero");
Equal(10f, new LevelProgressionConfig().GetEnemyHealthBonus(30, MonsterTier.Normal), "fallback flat health matches saved settings");
foreach (var profile in new[] { normal, hard })
foreach (var definition in definitions.Where(value => value.tier == MonsterTier.Elite))
foreach (int stage in Enumerable.Range(0, 5))
{
    Require(MonsterBalanceResolver.TryResolve(definition, "Default", profile, stages, stage, out var result, out _), "elite resolves");
    float expected = profile.difficulty == GameDifficulty.Hard ? 4f : 3f;
    Equal(expected, result.AttackPower, "elite hit damage independent of stage");
    foreach (var pattern in definition.patternDamage)
        Equal(pattern.coefficient > 0f ? expected : 0f, result.GetPatternDamage(pattern.coefficient), "elite pattern hit damage");
    if (definition.contact.enabled && definition.contact.damageCoefficient > 0f)
        Equal(expected, result.ContactDamage, "elite contact hit damage");
}
for (int seed = 0; seed < 250; seed++)
{
    var previous = new[] { "double_core", "triple_core", "exoskeleton" };
    var refreshed = CampaignRewardRules.RefreshOffers(previous, catalog, new[] { "triple_core" }, null, seed);
    Require(refreshed.Count == 3 && refreshed.Distinct().Count() == 3, "refilled reward has three distinct choices");
    Require(refreshed.Contains("double_core") && refreshed.Contains("exoskeleton") && !refreshed.Contains("triple_core"), "saved eligible offers remain stable");
    Require(refreshed.SequenceEqual(CampaignRewardRules.RefreshOffers(refreshed, catalog, new[] { "triple_core" }, null, seed + 1)), "reopening cannot reroll offers");
}
Require(!CampaignRewardRules.ShouldDropItem(MonsterTier.Normal, 0f), "normal cannot drop even at zero roll");
Require(!CampaignRewardRules.ShouldDropItem(MonsterTier.Normal, .01f), "normal drop upper boundary");
Require(CampaignRewardRules.ShouldDropItem(MonsterTier.Elite, 0f), "elite drop lower boundary");
Require(CampaignRewardRules.ShouldDropItem(MonsterTier.Elite, .1499f), "elite drops below fifteen percent");
Require(!CampaignRewardRules.ShouldDropItem(MonsterTier.Elite, .15f), "elite drop upper boundary");
Require(CampaignRewardRules.ShouldDropItem(MonsterTier.Boss, 1f), "boss always drops");
Require(!CampaignRewardRules.ShouldDropItem(MonsterTier.Normal, float.NaN), "invalid drop roll rejected");
foreach (int kills in new[] { 29, 30, 59, 60, 80, 90, 120, 5000 })
{
    int created = 0;
    while (CampaignRewardRules.RecoveryDue(kills, created)) created++;
    Equal(Math.Min(kills / 30, 3), created, "recovery cap at " + kills + " kills");
}
var worldReward = new WorldRewardSaveData
{ id = "region.Intestine.cache.0", biome = BiomeType.Intestine, collected = true, x = 4, z = 8,
  itemChoices = CampaignRewardRules.ChooseOffers(catalog, owned, "triple_core", 17) };
var restoredReward = JsonSerializer.Deserialize<WorldRewardSaveData>(JsonSerializer.Serialize(worldReward, json), json);
Require(restoredReward.collected && restoredReward.itemChoices.SequenceEqual(worldReward.itemChoices), "collected reward and offers survive serialization");
var encounters = new List<string>();
Require(CampaignRewardRules.TryUseEncounter(encounters, "region.Intestine.visit1"), "initial encounter trigger");
Require(!CampaignRewardRules.TryUseEncounter(encounters, "region.Intestine.visit1"), "same encounter cannot retrigger");
encounters = JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(encounters));
Require(!CampaignRewardRules.TryUseEncounter(encounters, "region.Intestine.visit1"), "encounter use survives reload");
Require(CampaignRewardRules.TryUseEncounter(encounters, "region.Lung.visit2"), "new encounter can trigger");
var metrics = new CampaignBalanceMetrics();
metrics.Tick(3, BiomeType.None, GameState.InHub);
metrics.Tick(1080, BiomeType.Intestine, GameState.InBiome);
metrics.Tick(240, BiomeType.Intestine, GameState.InBossRoom);
metrics.Tick(720, BiomeType.None, GameState.InFinalBoss);
metrics.Tick(0, BiomeType.Intestine, GameState.InBiome);
metrics.Tick(float.NaN, BiomeType.None, GameState.InHub);
metrics.TickSession(2100);
Equal(2043, metrics.activeSeconds, "active phase durations");
Equal(1080, metrics.Region(BiomeType.Intestine).fieldSeconds, "field duration");
Equal(240, metrics.Region(BiomeType.Intestine).bossSeconds, "regional boss duration");
Equal(720, metrics.finalBossSeconds, "final boss duration");
Equal(2100, metrics.sessionSeconds, "session duration includes pause time separately");
for (int i = 0; i < 2000; i++) metrics.Record("test", "item");
Equal(1024, metrics.events.Count, "bounded telemetry log");
Console.WriteLine($"PASS: {assertions} assertions against actual C# math and serialized asset inputs. Engine lifecycle/physics/UI not exercised.");
