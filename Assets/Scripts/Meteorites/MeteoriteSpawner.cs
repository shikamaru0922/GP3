using System.Collections.Generic;
using UnityEngine;

public class MeteoriteSpawner : MonoBehaviour
{
    public GameObject meteoritePrefab; // 陨石预制件
    public float spawnInterval = 5f;   // 生成间隔时间
    public float spawnDistance = 50f;  // 陨石生成的距离（玩家摄像机范围以外）
    public float min_spawnScale = 5f;
    public float max_spawnScale = 5f;
    public float speed = 5f;

    public List<Transform> spawnPositions; // 陨石可生成的位置列表

    private Transform playerTransform;
    private float timer = 0f;
    public PlayerController player;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            // 随机选择生成方法
            int spawnMethod = Random.Range(0, 2); // 0 或 1

            if (spawnMethod == 0)
            {
                SpawnMeteorite();
            }
            else
            {
                SpawnMeteoriteAtRandomPositions();
            }

            timer = 0f;
        }
    }

    void SpawnMeteorite()
    {
        if (player.gravityNum == 1)
            return;

        // 随机生成一个方向
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = Mathf.Abs(randomDirection.y); // 确保陨石从上方生成

        // 计算生成位置
        Vector3 spawnPosition = playerTransform.position + randomDirection * spawnDistance;

        // 实例化陨石
        GameObject meteorite = Instantiate(meteoritePrefab, spawnPosition, Quaternion.identity);

        // 随机调整陨石的大小
        float scale = Random.Range(min_spawnScale, max_spawnScale);
        meteorite.transform.localScale = Vector3.one * scale;

        // 调整陨石的速度
        Meteorite meteoriteScript = meteorite.GetComponent<Meteorite>();
        meteoriteScript.speed = speed;
    }

    void SpawnMeteoriteAtRandomPositions()
    {
        if (player.gravityNum == 1)
            return;

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("No spawn positions assigned!");
            return;
        }

        // 从指定的位置列表中随机选择一个位置
        int randomIndex = Random.Range(0, spawnPositions.Count);
        Transform spawnPoint = spawnPositions[randomIndex];

        // 实例化陨石
        GameObject meteorite = Instantiate(meteoritePrefab, spawnPoint.position, Quaternion.identity);

        // 随机调整陨石的大小
        float scale = Random.Range(min_spawnScale, max_spawnScale);
        meteorite.transform.localScale = Vector3.one * scale;

        // 调整陨石的速度
        Meteorite meteoriteScript = meteorite.GetComponent<Meteorite>();
        meteoriteScript.speed = speed;
    }
}
