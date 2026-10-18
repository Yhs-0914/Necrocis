using System;
using System.Globalization;
using UnityEngine;
namespace Necrocis
{
    // Placement identity; never an engine InstanceID or the global Random stream.
    public readonly struct EnemySpawnerIdentity : IEquatable<EnemySpawnerIdentity>
    {
        public readonly int Seed, Salt;
        public readonly BiomeType Biome;
        public readonly Vector2Int Cell;
        public readonly string RuleId;
        public readonly uint StableHash;
        public readonly string Key;
        public int EvaluationSlot => (int)(StableHash % 10u);
        public EnemySpawnerIdentity(int seed,BiomeType biome,Vector2Int cell,string ruleId,int salt)
        {
            Seed=seed;Biome=biome;Cell=cell;RuleId=ruleId??string.Empty;Salt=salt;
            uint hash=2166136261u;Mix(ref hash,seed);Mix(ref hash,(int)biome);Mix(ref hash,cell.x);Mix(ref hash,cell.y);Mix(ref hash,salt);
            foreach(char c in RuleId){unchecked{hash=(hash^(byte)c)*16777619u;hash=(hash^(byte)(c>>8))*16777619u;}}
            StableHash=hash==0?0x9e3779b9u:hash;
            Key=seed.ToString(CultureInfo.InvariantCulture)+"/"+((int)biome).ToString(CultureInfo.InvariantCulture)+"/"+cell.x.ToString(CultureInfo.InvariantCulture)+","+cell.y.ToString(CultureInfo.InvariantCulture)+"/"+salt.ToString(CultureInfo.InvariantCulture)+"/"+RuleId;
        }
        static void Mix(ref uint hash,int value){unchecked{uint bits=(uint)value;for(int i=0;i<4;i++){hash=(hash^(byte)bits)*16777619u;bits>>=8;}}}
        public bool Equals(EnemySpawnerIdentity other)=>Seed==other.Seed&&Biome==other.Biome&&Cell==other.Cell&&Salt==other.Salt&&string.Equals(RuleId,other.RuleId,StringComparison.Ordinal);
        public override bool Equals(object other)=>other is EnemySpawnerIdentity id&&Equals(id);
        public override int GetHashCode()=>unchecked((int)StableHash);
    }
    public struct SpawnPositionRandom
    {
        private uint state;
        public uint State { get=>state;set=>state=value==0?0x9e3779b9u:value; }
        public SpawnPositionRandom(uint seed){state=seed==0?0x9e3779b9u:seed;}
        private float NextUnit(){unchecked{uint x=state;x^=x<<13;x^=x>>17;x^=x<<5;state=x;return (x>>8)*(1f/16777216f);}}
        public Vector2 InsideUnitCircle(){float radius=Mathf.Sqrt(NextUnit());float angle=NextUnit()*(2f*Mathf.PI);return new Vector2(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);}
    }
}
