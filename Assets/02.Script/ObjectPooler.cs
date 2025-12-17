using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // 싱글턴 인스턴스: 게임 내 어디서든 ObjectPooler.Instance로 이 스크립트에 접근 가능
    public static ObjectPooler Instance;

    [SerializeField] private GameObject objectToPool; // 풀링할 프리팹 (투사체)
    [SerializeField] private int amountToPool; // 처음에 생성할 개수

    private List<GameObject> pooledObjects; // 풀링된 오브젝트들을 담을 리스트

    private void Awake()
    {
        Instance = this; // 싱글턴 인스턴스 할당
    }

    void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            obj.SetActive(false); // 비활성화 상태로 생성
            pooledObjects.Add(obj);
        }
    }

    // 풀에서 비활성화된 오브젝트를 찾아 반환하는 함수
    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        // 만약 모든 오브젝트가 사용 중이라면 null을 반환 (나중에 풀을 동적으로 늘리는 로직 추가 가능)
        return null;
    }
}