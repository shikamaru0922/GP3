using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionEffect;        // 爆炸特效
    public float explosionRadius = 5f;        // 爆炸半径
    public float explosionForce = 700f;       // 爆炸力
    public float upwardModifier = 1f;         // 向上修正值
    public AudioClip explosionSound;          // 爆炸音效

    private bool hasExploded = false;
    private bool explodedInMeteorite;
    public void Detonate()
{
    if (hasExploded) return;

    // 显示爆炸特效
    if (explosionEffect != null)
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }

    // 播放爆炸音效
    if (explosionSound != null)
    {
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
    }

    // 获取爆炸范围内的所有碰撞体
    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

    bool anyMeteoriteWithPlayerAttached = false; // 是否有陨石附着了玩家
    GameObject playerObject = null; // 玩家对象

    // 首先处理陨石
    foreach (Collider nearbyObject in colliders)
    {
        if (nearbyObject.gameObject.CompareTag("Meteorite"))
        {
            Meteorite meteorite = nearbyObject.GetComponent<Meteorite>();
            if (meteorite != null)
            {
                if (meteorite.playerAttached)
                {
                    anyMeteoriteWithPlayerAttached = true;
                    playerObject = meteorite.playerObject; // 从陨石中获取玩家对象
                }

                meteorite.DestroyMeteorite();
            }
        }
        else if (nearbyObject.gameObject.CompareTag("Player"))
        {
            // 如果 playerObject 还未被设置，则设置为当前的玩家对象
            if (playerObject == null)
            {
                playerObject = nearbyObject.gameObject;
            }
        }
    }

    // 如果玩家在爆炸范围内
    if (playerObject != null)
    {
        Rigidbody rb = playerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 计算从炸弹到玩家的方向
            Vector3 explosionDirection = (playerObject.transform.position - transform.position).normalized;

            // 如果有陨石附着了玩家，可以记录日志或执行其他逻辑
            if (anyMeteoriteWithPlayerAttached)
            {
                return;
                Debug.Log("Player was attached to a meteorite that was destroyed.");
            }
            // 施加爆炸力
            rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);

  
        }
    }

    hasExploded = true;
    Destroy(gameObject); // 销毁炸弹
}


}