using System;
using UnityEngine;
namespace Necrocis
{
    public interface IPlayerWorldSurface
    {
        float TileSize {get;}
        float GetGroundHeight(Vector3 position);
        bool CanMove(Vector3 from,Vector3 to);
    }
    public static class PlayerWorldBindings
    {
        public static Func<IPlayerWorldSurface> ResolveSurface;
        public static Func<Transform> ResolvePlayerTransform;
        public static Func<Vector3,Vector3,Vector2,bool> CanTraverse;
        public static Action<IPlayerTerrainQueries> ConnectTerrain;
        public static Action<IPlayerTerrainQueries> DisconnectTerrain;
    }
}
