using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionEffect;        // 爆炸特效
    public float explosionRadius = 5f;        // 爆炸半径
    public float explosionForce = 700f;       // 爆炸力
    public float upwardModifier = 1f;         // 向上修正值
    public AudioClip explosionSound;          // 爆炸音效

    private bool hasExploded = false;

    public void Detonate()
    {
        if (hasExploded) return;

        // 显示爆炸特效
        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // 播放爆炸音效
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        // 获取爆炸范围内的物体
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            Debug.Log("Player detected");
            if (rb != null)
            {
                if (rb.gameObject.CompareTag("Player"))
                {
                    
                    Vector3 explosionDirection = (rb.transform.position - transform.position).normalized;
                    rb.AddForce(explosionDirection.normalized * explosionForce, ForceMode.Impulse);
                }
                else
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
        }

        hasExploded = true;
        Destroy(gameObject); // 销毁炸弹
    }
}