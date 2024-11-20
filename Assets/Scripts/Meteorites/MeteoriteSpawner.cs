using UnityEngine;

public class MeteoriteSpawner : MonoBehaviour
{
    public GameObject meteoritePrefab; // 陨石预制件
    public float spawnInterval = 5f;   // 生成间隔时间
    public float spawnDistance = 50f;  // 陨石生成的距离（玩家摄像机范围以外）
    public float min_spawnScale = 5;
    public float max_spawnScale = 5;
    public float speed = 5f;
    private Transform playerTransform;
    private float timer = 0f;
    public PlayerController player;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMeteorite();
            timer = 0f;
        }
    }

    void SpawnMeteorite()
    {
        if (player.gravityNum ==1)
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
}