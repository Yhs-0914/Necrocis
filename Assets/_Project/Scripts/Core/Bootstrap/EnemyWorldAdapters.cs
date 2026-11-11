using UnityEngine;
namespace Necrocis
{
    public static class EnemyWorldAdapters
    {
        private sealed class Surface : IEnemyTerrainSurface
        {
            private readonly BiomeManager biome;
            public Surface(BiomeManager biome){this.biome=biome;}
            public bool IsAvailable=>biome!=null;
            public BiomeType BiomeType=>biome.BiomeType;
            public int MapWidth=>biome.MapWidth;public int MapHeight=>biome.MapHeight;public float TileSize=>biome.TileSize;
            public Vector2Int WorldToGrid(Vector3 p)=>biome.WorldToGrid(p);
            public Vector3 GridToWorld(int x,int y)=>biome.GridToWorld(x,y);
            public float GetGroundHeight(Vector3 p)=>biome.GetGroundHeight(p);
            public int GetHeightLevel(int x,int y)=>biome.GetHeightLevel(x,y);
            public bool IsValidPosition(int x,int y)=>biome.IsValidPosition(x,y);
            public bool IsWalkable(int x,int y)=>biome.IsWalkable(x,y);
            public bool CanMove(Vector3 from,Vector3 to)=>biome.CanMove(from,to);
            public Vector3 GetPlayerSpawnPosition()=>biome.GetPlayerSpawnPosition();
            public EnemySpawnContext Capture(EnemySpawnRuleConfig rule,Vector3 position,MonsterBalanceRuntime balance,EnemyEncounterRole role,bool ignoreRestrictions)
                =>EnemySpawnContextFactory.WithBalanceForBiome(biome,rule,position,balance,role,ignoreRestrictions);
        }
        private sealed class Attacks : IEnemyAttackWorldQueries
        {
            public Vector3 GetFlightPosition(Vector3 p,float h)=>EnemyProjectileWorldQueries.GetFlightPosition(p,h);
            public bool IsVisible(Vector3 p)=>EnemyProjectileWorldQueries.IsVisible(p);
            public bool HasClearPath(Vector3 a,Vector3 b)=>EnemyProjectileWorldQueries.HasClearPath(a,b);
            public bool HasClearFlightPath(Vector3 a,Vector3 b,float r)=>EnemyProjectileWorldQueries.HasClearFlightPath(a,b,r);
            public bool CanTravelDisk(Vector3 a,Vector3 b,float r)=>EnemyProjectileWorldQueries.CanTravelDisk(a,b,r);
        }
        private sealed class Encounters : IEnemyEncounterQueries
        {
            public bool IsInsideLockedArena(Vector3 p)=>EncounterSpatialQueries.IsInsideLockedArena(p);
            public bool TryClampPlayerMovement(Vector3 a,Vector3 b,float m,out Vector3 c)=>EncounterSpatialQueries.TryClampPlayerMovement(a,b,m,out c);
            public bool TryClampPosition(Vector3 p,float m,out Vector3 c)=>EncounterSpatialQueries.TryClampPosition(p,m,out c);
        }
        private sealed class Placement : IEnemyPlacementQueries
        {
            public bool CanPlaceResidue(Vector3 c,Vector3 f,Vector2 s)=>WorldResiduePlacementPolicy.CanPlace(c,f,s);
        }
        private static BiomeManager cachedBiome;
        private static Surface cachedSurface;
        public static void Register()
        {
            EnemyWorldBindings.ResolveSurface=()=>{
                var biome=BiomeManager.Active;if(biome==null){cachedBiome=null;cachedSurface=null;return null;}
                if(!ReferenceEquals(biome,cachedBiome)){cachedBiome=biome;cachedSurface=new Surface(biome);}return cachedSurface;};
            EnemyWorldBindings.Attacks=new Attacks();EnemyWorldBindings.Encounters=new Encounters();EnemyWorldBindings.Placement=new Placement();
            EnemySpawnBindings.Identity=EnemySpawnContextFactory.CaptureSpawnerIdentity;
            EnemySpawnBindings.Current=(r,p,b,role,ignore)=>EnemySpawnContextFactory.CaptureCurrent(r,p,b,role,ignore);
            EnemySpawnBindings.WithoutSurface=(r,p,b,role,ignore)=>EnemySpawnContextFactory.WithBalanceForBiome(null,r,p,b,role,ignore);
        }
    }
}
