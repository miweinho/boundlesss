using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input (New Input System)")]
    public InputAction MoveAction;
    public InputAction NextWeaponAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Combat/Aim")]
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private bool rotateHandToAim = true;

    private Camera cam;
    private Rigidbody2D rb;
    private Animator anim;
    private WeaponInventory inventory;

    private Vector2 move;
    private Vector2 lastMoveDirection = Vector2.right; // for idle facing / anim
    private Vector2 lastAim = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.Log("Big error no animator!!!!");
        }
        cam = Camera.main;
        inventory = GetComponent<WeaponInventory>();
    }

    void OnEnable()
    {
        MoveAction.Enable();
        NextWeaponAction.Enable();
        NextWeaponAction.started += _ => inventory?.NextWeapon();
    }

    void OnDisable()
    {
        MoveAction.Disable();
        NextWeaponAction.started -= _ => inventory?.NextWeapon(); // or keep a method handler
        NextWeaponAction.Disable();
    }

    void Update()
    {
        if (GameManager.Instance.GameplayActive)
        {
            // --- Movement input ---
            move = MoveAction.ReadValue<Vector2>();
            if (move.sqrMagnitude > 1f) move.Normalize(); // keep diagonal speed consistent

            // Track last non-zero move direction for animations
            if (move.sqrMagnitude > 0.0001f)
                lastMoveDirection = move;

            // --- Mouse aim (world space) ---
            if (weaponHolder != null && cam != null)
            {
                Vector2 mouseScreen = Mouse.current != null
                    ? Mouse.current.position.ReadValue()
                    : (Vector2)Input.mousePosition;

                Vector3 handPos = weaponHolder.HandTransform.position;
                float zDist = handPos.z - cam.transform.position.z; // works ortho/perspective
                Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, zDist));
                mouseWorld.z = handPos.z;

                Vector2 aim = ((Vector2)(mouseWorld - handPos));
                if (aim.sqrMagnitude > 0.0001f) lastAim = aim.normalized;
                // Feed aim to the weapon system
                weaponHolder.SetAim(lastAim);
                weaponHolder.HandTransform.right = lastAim;

                // Optional: rotate the hand bone/transform to point at the aim
                if (rotateHandToAim && weaponHolder.HandTransform != null)
                    weaponHolder.HandTransform.right = lastAim;

                // Fire on left mouse click (optional). Remove if you trigger elsewhere.
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    weaponHolder.TryAttack();
                }

            }

            
        }
        Animate();
    }

    void FixedUpdate()
    {
        // Physics movement
        Vector2 targetPos = rb.position + move * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    void Animate()
    {
        if (!anim) return;
        anim.SetFloat("MoveX", move.x);
        anim.SetFloat("MoveY", move.y);
        anim.SetFloat("MoveMagnitude", move.magnitude);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }
}
