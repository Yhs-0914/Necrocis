# Optimization And Folder Organization Summary

## Scope
This document summarizes the work completed before pushing to `main`.

The work covered four areas:

1. runtime stability cleanup
2. combat allocation reduction
3. enemy AI CPU-path optimization
4. folder organization

It does **not** cover new gameplay content or balance redesign.

---

## 1. Runtime Stability

### Goal
Reduce startup ambiguity and stop scene/runtime singleton conflicts from creating hard-to-reproduce bugs.

### What changed
- Reduced the Hub camera setup to a single active `DontStarveCamera`
- Attached `GameManager` and `InputManager` to the Hub-side runtime manager path
- Unified singleton behavior so duplicates destroy themselves consistently
- Added static-instance cleanup on destroy for major singleton-style systems
- Lowered `GameInitializer` side effects so it does not aggressively auto-spawn managers when scene instances already exist

### Files
- `Assets/_Project/Scenes/Hub.unity`
- `Assets/_Project/Scripts/Core/GameManager.cs`
- `Assets/_Project/Scripts/Core/SceneLoader.cs`
- `Assets/_Project/Scripts/Camera/DontStarveCamera.cs`
- `Assets/_Project/Scripts/Input/InputManager.cs`
- `Assets/_Project/Scripts/Player/PlayerController.cs`
- `Assets/_Project/Scripts/Player/PlayerStats.cs`
- `Assets/_Project/Scripts/Player/ObjectPooler.cs`
- `Assets/_Project/Scripts/Core/GameInitializer.cs`

### Expected result
- Fewer duplicate singleton cases
- More predictable scene transition state
- Lower risk of camera/player bootstrap conflicts

---

## 2. Combat Allocation Reduction

### Goal
Remove repeated `Instantiate`/`Destroy` usage from normal skill combat so frame spikes and GC churn are reduced during active fights.

### What changed
- Added a shared runtime pool for temporary combat objects
- Reworked skill projectile spawning to acquire pooled objects instead of creating new ones each cast
- Reworked skill hit and temporary effect spawning to use pooled objects
- Converted `SkillProjectile` expiry and hit cleanup to pool return when possible
- Replaced global enemy enumeration fallback with active enemy list usage

### Files
- `Assets/_Project/Scripts/Core/RuntimePool.cs`
- `Assets/_Project/Scripts/Player/Skills/PlayerClassSkillController.cs`
- `Assets/_Project/Scripts/Player/Skills/SkillProjectile.cs`
- `Assets/_Project/Scripts/Enemy/EnemyController.cs`

### Expected result
- Lower GC allocations during repeated skill use
- More stable frame pacing in dense combat
- Less responsiveness loss when chaining class skills

---

## 3. Enemy AI CPU Optimization

### Goal
Reduce CPU cost in enemy-heavy situations without changing enemy aggression, pacing, or attack feel.

### What changed
- Removed per-frame full-scene enemy lookup from `AggroDebris`
- Added active enemy list access for localized enemy queries
- Replaced full active-enemy separation scans with a simple spatial hash neighborhood lookup
- Cached enemy ground height by tile/grid position instead of sampling every frame unconditionally
- Centralized active camera lookup so hot paths do not depend on repeated `Camera.main` access

### Files
- `Assets/_Project/Scripts/Enemy/AggroDebris.cs`
- `Assets/_Project/Scripts/Enemy/EnemyController.cs`
- `Assets/_Project/Scripts/Enemy/EnemySpawner.cs`
- `Assets/_Project/Scripts/Enemy/EnemyProjectile.cs`
- `Assets/_Project/Scripts/Camera/Billboard.cs`
- `Assets/_Project/Scripts/Camera/DontStarveCamera.cs`

### Expected result
- Better scaling as enemy count rises
- Lower CPU overhead in aggro-heavy and crowd-heavy scenes
- Less wasted work on camera and terrain height lookups

---

## 4. Render Optimization

### Goal
Apply low-risk renderer changes that reduce GPU cost without noticeably harming readability.

### What changed
- Disabled PC SSAO
- Disabled PC depth texture requirement
- Disabled PC opaque texture requirement
- Disabled additional light shadows on PC
- Reduced PC shadow distance
- Reduced PC shadow cascade count

### Files
- `Assets/_Project/Settings/PC_RPAsset.asset`
- `Assets/_Project/Settings/PC_Renderer.asset`

### Expected result
- Lower GPU cost in general play
- Lower shadow and screen-space overhead
- Minimal impact on gameplay readability

---

## 5. Folder Organization

### Goal
Separate project-owned assets from third-party assets and create a clearer top-level structure for future work.

### New top-level structure

```text
Assets/
├── _Project
│   ├── Art
│   ├── Audio
│   ├── Data
│   ├── Docs
│   ├── Prefabs
│   ├── Resources
│   ├── Scenes
│   ├── Scripts
│   └── Settings
└── _ThirdParty
    ├── EVil Wizard
    ├── Pixel Art
    └── TutorialInfo
```

### What moved
- scenes into `Assets/_Project/Scenes`
- scripts into `Assets/_Project/Scripts`
- prefabs into `Assets/_Project/Prefabs`
- images, tiles, fonts, animations, materials, shaders into `Assets/_Project/Art`
- biome config assets into `Assets/_Project/Data/BiomeConfigs`
- docs into `Assets/_Project/Docs`
- third-party packs into `Assets/_ThirdParty`

### Follow-up updates
- Updated `ProjectSettings/EditorBuildSettings.asset` scene paths
- Updated internal docs to use the new paths
- Added `Assets/_Recovery/` to `.gitignore`

---

## Verification Notes

### Completed
- Static diff validation on the modified code/config files
- Path updates for build settings and docs
- User play-check reported no visible gameplay issues during testing

### Still recommended after pull/push
- Open Hub and all biome scenes in Unity once
- Confirm Build Settings still list valid scene paths
- Confirm no missing references in moved prefabs and ScriptableObjects
- Re-run a quick combat sanity pass for Mage and Archer skills

---

## Commit Intent

Recommended commit message:

`optimization and folder organization`
