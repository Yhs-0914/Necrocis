using UnityEngine;
namespace Necrocis
{
    // Warning, flexible membrane and damage share the same terrain-clipped sector.
    public sealed class OilFilmSector
    {
        public const int Segments = 64;
        private const float TerrainInset = .02f;
        public readonly Vector3 Origin, Forward, Right;
        public readonly float Range, Arc;
        private readonly BiomeManager biome;
        private readonly int heightLevel;
        private readonly float[] limits = new float[Segments + 1], scratch = new float[Segments + 1];
        public bool TerrainClipped { get; private set; }
        public float MaximumReach { get; private set; }
        public OilFilmSector(Vector3 origin, Vector3 direction, float range, float arc)
        {
            Origin = origin; Forward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            Right = Vector3.Cross(Vector3.up, Forward); Range = range; Arc = arc;
            biome = BiomeManager.Active;
            if (biome != null) { var cell = biome.WorldToGrid(origin); heightLevel = biome.GetHeightLevel(cell.x, cell.y); }
            for (int i = 0; i < limits.Length; i++) limits[i] = range;
            MaximumReach = range; ReduceToCurrentTerrain();
        }
        private Vector3 Direction(float u)
        {
            float a = (u - .5f) * Arc * Mathf.Deg2Rad;
            return Forward * Mathf.Cos(a) + Right * Mathf.Sin(a);
        }
        public float RadiusAtAngle(float u)
        {
            float sample = Mathf.Clamp01(u) * Segments; int lo = Mathf.Min(Segments - 1, Mathf.FloorToInt(sample));
            return Mathf.Lerp(limits[lo], limits[lo + 1], sample - lo);
        }
        public void CopyLimits(float[] destination) => System.Array.Copy(limits, destination, limits.Length);
        public Vector3 UnclippedPoint(float angle01, float radius01) => Origin + Direction(angle01) * (Range * radius01);
        public Vector3 Point(float angle01, float radius01) => Origin + Direction(angle01) * (RadiusAtAngle(angle01) * radius01);
        public bool Contains(Vector3 point)
        {
            Vector3 p = point - Origin; p.y = 0;
            if (p.sqrMagnitude < .000001f) return MaximumReach > .001f;
            float angle = Mathf.Atan2(Vector3.Dot(p, Right), Vector3.Dot(p, Forward)) * Mathf.Rad2Deg;
            if (Mathf.Abs(angle) > Arc * .5f + .00001f) return false;
            float reach = RadiusAtAngle(angle / Arc + .5f);
            return p.sqrMagnitude <= reach * reach + .000001f;
        }
        // A new obstruction can shrink a committed warning, never enlarge it after a wall disappears.
        public bool ReduceToCurrentTerrain()
        {
            if (biome == null) return false;
            for (int i = 0; i <= Segments; i++)
            {
                float u = i / (float)Segments, halfStep = .5f / Segments;
                scratch[i] = Mathf.Min(RayLimit(Direction(u)), Mathf.Min(RayLimit(Direction(Mathf.Max(0, u-halfStep))), RayLimit(Direction(Mathf.Min(1, u+halfStep)))));
            }
            bool changed = false; MaximumReach = 0;
            for (int i = 0; i <= Segments; i++)
            {
                // Conservative neighbours prevent triangles bridging an obstacle corner between rays.
                float safe = Mathf.Min(scratch[i], Mathf.Min(scratch[Mathf.Max(0,i-1)], scratch[Mathf.Min(Segments,i+1)]));
                float next = Mathf.Min(limits[i], safe);
                if (next < limits[i] - .00001f) { limits[i] = next; changed = true; }
                MaximumReach = Mathf.Max(MaximumReach, limits[i]);
                TerrainClipped |= limits[i] < Range - .0001f;
            }
            return changed;
        }
        private bool ClearCell(int x, int z)
        {
            return biome.IsValidPosition(x,z) && biome.IsWalkable(x,z) && biome.GetHeightLevel(x,z)==heightLevel
                && !MidBossArenaController.IsPlayerInsideLockedArena(biome.GridToWorld(x,z));
        }
        private float RayLimit(Vector3 direction)
        {
            var cell = biome.WorldToGrid(Origin); int x=cell.x,z=cell.y;
            if (!ClearCell(x,z)) return 0;
            float tile=biome.TileSize;var center=biome.GridToWorld(x,z);
            int sx=direction.x>=0?1:-1,sz=direction.z>=0?1:-1;
            float dx=Mathf.Abs(direction.x)>.000001f?tile/Mathf.Abs(direction.x):float.PositiveInfinity;
            float dz=Mathf.Abs(direction.z)>.000001f?tile/Mathf.Abs(direction.z):float.PositiveInfinity;
            float tx=float.IsPositiveInfinity(dx)?dx:Mathf.Max(0,(center.x+sx*tile*.5f-Origin.x)/direction.x);
            float tz=float.IsPositiveInfinity(dz)?dz:Mathf.Max(0,(center.z+sz*tile*.5f-Origin.z)/direction.z);
            while (Mathf.Min(tx,tz)<=Range)
            {
                float at=Mathf.Min(tx,tz);bool crossX=tx<=tz+.000001f,crossZ=tz<=tx+.000001f;
                if ((crossX&&!ClearCell(x+sx,z)) || (crossZ&&!ClearCell(x,z+sz)) || (crossX&&crossZ&&!ClearCell(x+sx,z+sz))) return Mathf.Max(0,at-TerrainInset);
                if(crossX){x+=sx;tx+=dx;}if(crossZ){z+=sz;tz+=dz;}
            }
            return Range;
        }
        public void WriteVertices(Mesh mesh)
        {
            var vertices = mesh.vertices;
            for(int i=0;i<=Segments;i++)vertices[i+1]=Point(i/(float)Segments,1)-Origin;
            mesh.vertices=vertices;mesh.RecalculateBounds();
        }
        public Mesh MakeMesh()
        {
            var v = new Vector3[Segments + 2]; var colors = new Color[v.Length]; var triangles = new int[Segments * 3];
            for (int i = 0; i <= Segments; i++) v[i + 1] = Point(i / (float)Segments, 1) - Origin;
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            for (int i = 0; i < Segments; i++) { triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = i + 2; }
            var mesh = new Mesh { name = "OilFilmFilledSector", vertices = v, uv = new Vector2[v.Length], colors = colors, triangles = triangles };
            mesh.RecalculateBounds(); return mesh;
        }
    }
}
