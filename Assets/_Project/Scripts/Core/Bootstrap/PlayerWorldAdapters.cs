using System.Runtime.CompilerServices;
using UnityEngine;
namespace Necrocis
{
    public static class PlayerWorldAdapters
    {
        private sealed class Surface : IPlayerWorldSurface
        {
            private readonly BiomeManager biome;
            public Surface(BiomeManager biome){this.biome=biome;}
            public float TileSize=>biome.TileSize;
            public float GetGroundHeight(Vector3 position)=>biome.GetGroundHeight(position);
            public bool CanMove(Vector3 from,Vector3 to)=>biome.CanMove(from,to);
        }
        private sealed class Connection {public ProceduralTerrainMotor Motor;}
        private static readonly ConditionalWeakTable<IPlayerTerrainQueries,Connection> connections=new ConditionalWeakTable<IPlayerTerrainQueries,Connection>();
        private static BiomeManager cachedBiome;
        private static Surface cachedSurface;
        public static void Register()
        {
            PlayerWorldBindings.ResolveSurface=()=>{
                var biome=BiomeManager.Active;if(biome==null){cachedBiome=null;cachedSurface=null;return null;}
                if(!ReferenceEquals(cachedBiome,biome)){cachedBiome=biome;cachedSurface=new Surface(biome);}return cachedSurface;};
            PlayerWorldBindings.ResolvePlayerTransform=()=>PlayerController.Instance!=null?PlayerController.Instance.transform:null;
            PlayerWorldBindings.CanTraverse=EncounterSpatialQueries.CanPlayerTraverse;
            PlayerWorldBindings.ConnectTerrain=Connect;
            PlayerWorldBindings.DisconnectTerrain=Disconnect;
        }
        private static void Connect(IPlayerTerrainQueries terrain)
        {
            var player=PlayerController.Instance;
            if(player==null || !player.TryGetComponent(out ProceduralTerrainMotor motor))return;
            var state=connections.GetValue(terrain,_=>new Connection());
            if(state.Motor!=null && state.Motor!=motor)state.Motor.ClearTerrainContext(terrain);
            state.Motor=motor;motor.SetTerrainContext(terrain);
        }
        private static void Disconnect(IPlayerTerrainQueries terrain)
        {
            if(!connections.TryGetValue(terrain,out var state))return;
            if(state.Motor!=null)state.Motor.ClearTerrainContext(terrain);
            state.Motor=null;connections.Remove(terrain);
        }
    }
}
