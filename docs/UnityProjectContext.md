# Unity project context

Analyzed 2026-11-04 at `d1bf8ece28c86c9f9fd273efd698f951e42262c6` on gustlr02.

- Confirmed Unity 6000.3.9f1; URP 17.3.0, Input System 1.18.0, uGUI 2.0.0, Test Framework 1.6.0. No first-party asmdefs or automated test assemblies found.
- Build scenes: MainMenu → Hub → Intestine/Liver/Stomach/Lung. GameManager persists between scenes. README describes integrated boss and combat architecture.
- Main code: `Assets/_Project/Scripts`; legacy PlayerClass under `Assets/02.Scripts/Player`. Existing editor batch runners live in `Assets/_Project/Editor`.
- Progression: static LevelUpManager loads Resources/Balance/LevelProgressionConfig. ClassChoiceUI handles job selection; LevelUpUI handles queued stat choices; PlayerClassSkillController checks LevelUpManager for E/R unlocks. Existing XP formula and kill XP differ intentionally.
- Items: PlayerItemManager owns catalog and acquired entries, PlayerItemBase supplies ScriptableObject effects, PlayerItemCombatEffects handles ID-based combat behaviors. WorldItemSpawner generates floor boxes; MidBossArenaController also calls single-item spawn after boss death. No ordinary enemy item drop caller found.
- Enemies: pooled EnemyController partials, FSM, EnemySpawnRuleConfig nested in BiomeConfig.cs; ConfigurableBiomeManager registers elite rules separately. EnemySpawner owns ordinary waves. EliteSpawner owns kill-triggered elite generation.
- SpriteFrameAnimator uses seconds per frame, despite the name attackAnimationSpeed. Attack cooldown begins after one-shot completion, or immediately after damage without animation. First attack is immediate. Charge cooldown was hardcoded to 3 seconds.
- Bosses: MidBossArenaController builds separate rule copies and configures EnemyController directly. Pattern classes use PlayAttackAnimationOnly/PlayPatternAnimation; preserve these paths.
- Difficulty: GameDifficulty originally had only Normal; GameManager has the serialized owner. No difficulty multipliers or selection menu found.
- Unity Editor 6000.3.9f1 is installed locally. No Unity MCP tools exposed. Use batch compilation and an Editor validation runner; interactive visual/play behavior still needs appropriate testing.
- Constraints: only gustlr02 commits/push; preserve original meta GUIDs, scene/prefab structure, boss patterns and unspecified balance values.
