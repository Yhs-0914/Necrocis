// Audit probe: uses actual game math with the BalanceChecks engine shim. Run from repository root.
using Necrocis;
using System.Text.Json;
using var doc = JsonDocument.Parse(File.ReadAllText("output/balance/validation_inputs.json"));
DifficultyBalanceService.Progression = doc.RootElement.GetProperty("growth").Deserialize<LevelProgressionConfig>(new JsonSerializerOptions { IncludeFields = true });
int statPrompts = 0, jobPrompts = 0;
LevelUpManager.OnLevelUp = () => statPrompts++;
LevelUpManager.OnJobSelect = () => jobPrompts++;
LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
foreach (int amount in new[] {30,50,70,90}) LevelUpManager.AddResolvedEnemyKillExp(amount);
LevelUpManager.SetJob(JobType.Mage);
while (LevelUpManager.HasPendingLevelUp()) LevelUpManager.ProcessNextPendingLevelUp();
var atFive = new { level = LevelUpManager.GetCurrentLevel(), statPrompts, jobPrompts };
LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
statPrompts = 0;
LevelUpManager.AddResolvedEnemyKillExp(150);
var save = new PlayerProgressSaveData(); LevelUpManager.CaptureProgress(save);
bool pendingBefore = LevelUpManager.HasPendingLevelUp();
LevelUpManager.RestoreProgress(save);
var restore = new { level = LevelUpManager.GetCurrentLevel(), pendingBefore, pendingAfter = LevelUpManager.HasPendingLevelUp(), emittedStatPrompts = statPrompts };
LevelUpManager.RestoreProgress(new PlayerProgressSaveData());
statPrompts = 0;
LevelUpManager.AddResolvedEnemyKillExp(30);
LevelUpManager.AddResolvedEnemyKillExp(50);
var repeated = new { level = LevelUpManager.GetCurrentLevel(), emittedStatPromptsWithoutSelection = statPrompts, pending = LevelUpManager.HasPendingLevelUp() };
var options = new JsonSerializerOptions { IncludeFields = true };
var profile = doc.RootElement.GetProperty("normal").Deserialize<DifficultyBalanceProfile>(options);
var progression = doc.RootElement.GetProperty("stages").Deserialize<MonsterProgressionProfile>(options);
var definitions = doc.RootElement.GetProperty("definitions").Deserialize<List<MonsterDefinition>>(options);
var xpRows = new List<object>();
for (int stage=0;stage<5;stage++) foreach (int baseXp in new[]{10,12,15})
{
    var definition = definitions.First(d => d.tier == MonsterTier.Normal && d.reward.baseExperience == baseXp);
    if (!MonsterBalanceResolver.TryResolve(definition,"Default",profile,progression,stage,out var resolved,out var error)) throw new Exception(error);
    xpRows.Add(new { stage, baseXp, resolved.Experience });
}
string result = JsonSerializer.Serialize(new { atFive, restore, repeated, xpRows },new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("Docs/Balance/REVIEW_PROBE_2026-10-26.json", result);
Console.WriteLine(result);
