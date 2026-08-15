namespace ProceduralMap.Pooling
{
    /// <summary>풀에서 꺼내거나 반환될 때 오브젝트가 자체 상태를 초기화하는 공통 규약.</summary>
    public interface IPoolable
    {
        void OnTakenFromPool();
        void OnReturnedToPool();
    }
}
