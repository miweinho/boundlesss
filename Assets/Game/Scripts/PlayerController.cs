using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;

    Rigidbody2D rigidbody2d;
    public Vector2 move;

    public int maxHealth = 5;

    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float damageCooldown;

    //Variables for Animator
    Animator anim;
    private Vector2 lastMoveDirection;

    // Start is called before the first frame update
    void Start()
    {
        //Set the player to 0,0 
        Vector2 startPos = new(0.0f, 0.0f);
        transform.position = startPos;
        MoveAction.Enable();
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        move.Normalize();
        Animate();
        //Debug.Log(move);

        if (isInvincible)
        {
            damageCooldown -= Time.deltaTime;
            Debug.Log(damageCooldown);
            if (damageCooldown <= 0)
            {
                isInvincible = false;
            }
        }




    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * 3.0f * Time.deltaTime;
        rigidbody2d.MovePosition(position);
        Camera.main.transform.position = new(position.x, position.y, -10.0f);
        if (move.x <= 0.01 && move.y <= 0.01)
        {
            lastMoveDirection = move;
        }
        

    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0 && !isInvincible)
        {
            isInvincible = true;
            damageCooldown = timeInvincible;
        
        }
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
    }

    public int health
    {
        get
        {
            return currentHealth;
        }
    }

    void Animate()
    {
        anim.SetFloat("MoveX", move.x);
        anim.SetFloat("MoveY", move.y);
        anim.SetFloat("MoveMagnitude", move.magnitude);
        anim.SetFloat("LastMoveX", lastMoveDirection.x);
        anim.SetFloat("LastMoveY", lastMoveDirection.y);
    }
}
