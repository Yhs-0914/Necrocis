using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public Transform player;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    void Start()
    {
        // 초기 스폰
        SpawnEnemies();
    }
    
    void Update()
    {
        // 죽은 적 리스트에서 제거
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        // 적이 부족하면 보충
        if (activeEnemies.Count < maxEnemies)
        {
            int spawnCount = maxEnemies - activeEnemies.Count;
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy();
            }
        }
    }
    
    void SpawnEnemies()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }
    
    void SpawnEnemy()
    {
        // 플레이어 주변 랜덤 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = player.position + new Vector3(randomCircle.x, 1, randomCircle.y);
        
        // 적 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
    }
}