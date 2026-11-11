using System;
using UnityEngine;
namespace Necrocis
{
    public interface IEnemyTerrainSurface
    {
        bool IsAvailable {get;}
        BiomeType BiomeType {get;}
        int MapWidth {get;} int MapHeight {get;} float TileSize {get;}
        Vector2Int WorldToGrid(Vector3 point);
        Vector3 GridToWorld(int x,int y);
        float GetGroundHeight(Vector3 point);
        int GetHeightLevel(int x,int y);
        bool IsValidPosition(int x,int y);
        bool IsWalkable(int x,int y);
        bool CanMove(Vector3 from,Vector3 to);
        Vector3 GetPlayerSpawnPosition();
        EnemySpawnContext Capture(EnemySpawnRuleConfig rule,Vector3 position,MonsterBalanceRuntime balance,EnemyEncounterRole role,bool ignoreRestrictions);
    }
    public interface IEnemyAttackWorldQueries
    {
        Vector3 GetFlightPosition(Vector3 position,float height);
        bool IsVisible(Vector3 point);
        bool HasClearPath(Vector3 from,Vector3 to);
        bool HasClearFlightPath(Vector3 from,Vector3 to,float radius);
        bool CanTravelDisk(Vector3 from,Vector3 to,float radius);
    }
    public interface IEnemyEncounterQueries
    {
        bool IsInsideLockedArena(Vector3 position);
        bool TryClampPlayerMovement(Vector3 from,Vector3 to,float margin,out Vector3 clamped);
        bool TryClampPosition(Vector3 point,float margin,out Vector3 clamped);
    }
    public interface IEnemyPlacementQueries {bool CanPlaceResidue(Vector3 center,Vector3 forward,Vector2 size);}
    public static class EnemyWorldBindings
    {
        public static Func<IEnemyTerrainSurface> ResolveSurface;
        public static IEnemyTerrainSurface Surface=>ResolveSurface();
        public static IEnemyAttackWorldQueries Attacks;
        public static IEnemyEncounterQueries Encounters;
        public static IEnemyPlacementQueries Placement;
    }
    public static class EnemySpawnBindings
    {
        public static Func<EnemySpawnRuleConfig,Vector3,EnemySpawnerIdentity> Identity;
        public static EnemySpawnerIdentity CaptureIdentity(EnemySpawnRuleConfig rule,Vector3 anchor)
            => Identity != null ? Identity(rule,anchor) : new EnemySpawnerIdentity(0,BiomeType.None,
                new Vector2Int(Mathf.RoundToInt(anchor.x*1024f),Mathf.RoundToInt(anchor.z*1024f)),
                rule?.monsterDefinition?.monsterId ?? rule?.name,rule?.poissonSalt??0);
        public static Func<EnemySpawnRuleConfig,Vector3,MonsterBalanceRuntime,EnemyEncounterRole,bool,EnemySpawnContext> Current;
        public static Func<EnemySpawnRuleConfig,Vector3,MonsterBalanceRuntime,EnemyEncounterRole,bool,EnemySpawnContext> WithoutSurface;
        public static EnemySpawnContext CaptureCurrent(EnemySpawnRuleConfig rule,Vector3 position,MonsterBalanceRuntime balance=null,EnemyEncounterRole role=EnemyEncounterRole.Field,bool ignoreEncounterMovementRestrictions=false)
            =>Current(rule,position,balance,role,ignoreEncounterMovementRestrictions);
        public static EnemySpawnContext CaptureForSurface(IEnemyTerrainSurface surface,EnemySpawnRuleConfig rule,Vector3 position,EnemyEncounterRole role=EnemyEncounterRole.Field,bool ignoreEncounterMovementRestrictions=false)
            =>surface!=null?surface.Capture(rule,position,null,role,ignoreEncounterMovementRestrictions):WithoutSurface(rule,position,null,role,ignoreEncounterMovementRestrictions);
    }
}
