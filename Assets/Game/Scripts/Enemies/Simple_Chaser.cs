using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simple_Chaser : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private float attackRange = 2f;

    Rigidbody2D rb;
    private float chaseSpeed = 3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Top-down friendly defaults
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void Update()
    {
        if (!target) return;
        Vector2 dir = (target.position - weaponHolder.HandTransform.position).normalized;
        weaponHolder.SetAim(dir);

        if (Vector2.Distance(target.position, transform.position) <= attackRange)
            weaponHolder.TryAttack();


        // Calculate direction towards the player
        Vector2 desiredDir = ((Vector2)target.position - rb.position).normalized;

        // Move towards the player
        Vector2 next = rb.position + desiredDir * chaseSpeed * Time.deltaTime;
        rb.MovePosition(next);
    }

    public void SetMoveSpeed(float s) => chaseSpeed = s;

    public void SetTarget(Transform t) => target = t;
}
