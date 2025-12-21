using UnityEngine;

public class BossSylasController : SkeletonController
{
    [Header("Boss Specifics")]
    [SerializeField] private GameObject exitGate; 
    [SerializeField] private Projectile arrowPrefab;
    [SerializeField] private float fanAttackCooldown = 2f;
    
    [Header("Projectile Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float arrowSpeed = 8f;
    [SerializeField] private float knockback = 2f;

    private float nextAttackTime;

    protected override void FixedUpdate()
    {
        // Deixamos vazio. Assim ele não persegue o jogador, mas roda o corpo.
        // Se o SkeletonController rodar o corpo no Update, ele vai continuar a mirar.
    }

    private void Update()
    {
        if (target == null) return;

        Vector2 myPos = transform.position;
        Vector2 toTarget = (Vector2)target.position - myPos;
        
        if (sr != null && Mathf.Abs(toTarget.x) > 0.01f)
            sr.flipX = toTarget.x < 0f;

        float dist = Vector2.Distance(target.position, transform.position);
        
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            PerformFanAttack(toTarget.normalized);
            nextAttackTime = Time.time + fanAttackCooldown;
        }
    }

    void PerformFanAttack(Vector2 mainDirection)
    {
        // Ângulos: -15, 0 (centro), +15
        float[] angles = { -15f, 0f, 15f };
        int enemyTeamID = 1;

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector2 dir = rot * mainDirection;

            Vector3 spawnPos = transform.position; 
            if (GetComponent<WeaponHolder>()) 
                spawnPos = GetComponent<WeaponHolder>().HandTransform.position;

            Projectile p = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

            p.Fire(dir, damage, knockback, enemyTeamID, 1f, GetComponent<Collider2D>());
        }
    }

    void OnDestroy()
    {
        if (exitGate != null)
        {
            exitGate.SetActive(false);
            Debug.Log("Sylas derrotado. O portão abriu-se.");
        }
    }
}