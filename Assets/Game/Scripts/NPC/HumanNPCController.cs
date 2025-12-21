using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Damageable))]
public class HumanNPCController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    [SerializeField] private float pauseDuration = 0.5f;

    [Header("Pace Pattern (relative to start)")]
    [SerializeField] private Vector2[] paceOffsets = new Vector2[]
    {
        new Vector2(1.5f, 0f),
        new Vector2(1.5f, 1f),
        new Vector2(-1.5f, 1f),
        new Vector2(-1.5f, 0f)
    };

    [Header("Combat Team")]
    [SerializeField] private int team = 0; // same as player

    // TODO: hook up interaction (dialogue, quests, etc.)

    private Rigidbody2D rb;
    private Damageable damageable;
    private SpriteRenderer sr;

    private Vector2 startPos;
    private int paceIndex;
    private bool returningToStart;
    private float pauseTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;

        if (damageable != null)
            damageable.team = team;
    }

    void Start()
    {
        startPos = rb.position;
        paceIndex = 0;
        returningToStart = false;
        pauseTimer = 0f;
    }

    void FixedUpdate()
    {
        if (paceOffsets == null || paceOffsets.Length == 0)
            return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 targetPos = returningToStart
            ? startPos
            : startPos + paceOffsets[paceIndex];

        Vector2 toTarget = targetPos - rb.position;
        float dist = toTarget.magnitude;

        if (dist <= arrivalThreshold)
        {
            if (returningToStart)
            {
                returningToStart = false;
                paceIndex = 0;
            }
            else if (paceIndex >= paceOffsets.Length - 1)
            {
                returningToStart = true;
            }
            else
            {
                paceIndex++;
            }

            pauseTimer = pauseDuration;
            return;
        }

        Vector2 dir = toTarget / dist;
        Vector2 velocity = dir * moveSpeed;

        if (sr != null && Mathf.Abs(velocity.x) > 0.01f)
            sr.flipX = velocity.x < 0f;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}
