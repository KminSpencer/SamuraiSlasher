using UnityEngine;
using System.Collections;

public class scr_CharacterController : MonoBehaviour
{
    //Sprite
    public SpriteRenderer sprite;
    //States
    enum states
    {
        normal,
        air,
        wall,
        forwardAttack,
        upAttack,
        downAttack,
        backAttack,
        airForwardAttack,
        airUpAttack,
        airDownAttack,
        airBackAttack,
        hurt,
        dead,
        takingAPoop,
        count
    }
    private states state = states.normal;
    private states currentState;
    private int frame;

    //Movement Component variables
    public float m_Acceleration;
    public float m_AirAcceleration;
    public float m_Friction;
    public float m_MaxSpeed;
    public float m_MaxDashSpeed;
    public float m_DashAcceleration;
    public float m_JumpForce;
    public float m_JumpGravity;
    public float m_NormalGravity;
    public float m_FallGravity;
    public float m_MaxGravity;
    public float m_WallGravity;
    public float m_MaxWallGravity;

    //Input
    private float HorizontalInput;
    private float VerticalInput;
    private bool Jump;
    private bool JumpIsDown;
    private bool Duck;
    private bool DuckIsDown;
    private bool Dash;
    private bool ForwardAttackButton;

    //Private Movement variables
    private Rigidbody m_Body;
    private Vector3 m_Velocity = Vector2.zero;
    private float m_Gravity;

    //Some wall running shit
    private float m_CurrentMaxWallRunSpeed;
    private bool m_CanJumpOnWall;
    private bool m_CanRunUpWall;

    //Collisions and shit
    [SerializeField]
    private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

    private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
    private Transform m_WallCheckLeft;
    private Transform m_WallCheckRight;
    private Transform m_CeilingCheck;
    const float k_GroundedRadius = .5f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private bool m_WallRight;
    private bool m_WallLeft;
    private bool m_Ceiling;

    //Raycast
    private Ray ray;

    //Hitbox
    public Transform m_Hitboxes;
    public Collider[] attackHitboxes;
    private void Awake()
    {
        // Setting up references
        m_Body = GetComponent<Rigidbody>();
        m_GroundCheck = transform.Find("GroundCheck");
        m_WallCheckLeft = transform.Find("WallCheckLeft");
        m_WallCheckRight = transform.Find("WallCheckRight");
        m_CeilingCheck = transform.Find("CeilingCheck");

    }

    // Use this for initialization
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        #region Check for ground and walls
        m_Grounded = false;
        Collider[] colliders = Physics.OverlapSphere(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
        }

        m_WallLeft = false;
        m_WallRight = false;
        Collider[] wallCollidersLeft = Physics.OverlapSphere(m_WallCheckLeft.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < wallCollidersLeft.Length; i++)
        {
            if (wallCollidersLeft[i].gameObject != gameObject)
                m_WallLeft = true;
        }
        Collider[] wallCollidersRight = Physics.OverlapSphere(m_WallCheckRight.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < wallCollidersRight.Length; i++)
        {
            if (wallCollidersRight[i].gameObject != gameObject)
                m_WallRight = true;
        }
        m_Ceiling = false;
        Collider[] ceilingColliders = Physics.OverlapSphere(m_CeilingCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < ceilingColliders.Length; i++)
        {
            if (ceilingColliders[i].gameObject != gameObject)
                m_Ceiling = true;
        }

        #endregion

        if(transform.position.z != 0f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
        

    }
    // Update is called once per frame
    void Update()
    {
        
        #region Update Input
        //Update Input
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");
        Jump = Input.GetButtonDown("Jump");
        Duck = Input.GetButtonDown("Duck");
        DuckIsDown = Input.GetButton("Duck");
        JumpIsDown = Input.GetButton("Jump");
        Dash = Input.GetButton("Dash");

        if (sprite.flipX)
        {
            ForwardAttackButton = Input.GetKeyDown(KeyCode.RightArrow);
        }else
        {
            ForwardAttackButton = Input.GetKeyDown(KeyCode.LeftArrow);
        }

        #endregion

        currentState = state;

        //Switch state
        switch (state)
        {
            case states.normal: Normal(); break;
            case states.air: Air(); break;
            case states.wall: Wall(); break;
            case states.forwardAttack: ForwardAttack(6,2,10);break;
                
        }
        //Move
        m_Body.velocity = new Vector3(m_Velocity.x, m_Velocity.y);


    }
    void LateUpdate()
    {
        
        //Reset Frame
        if (state != currentState)
        {
            frame = 0;
        }

        if (sprite.flipX)
        {
            m_Hitboxes.localRotation = Quaternion.Euler(0f, 0f, sprite.transform.localEulerAngles.z);
        }
        else
        {
            m_Hitboxes.localRotation = Quaternion.Euler(0f, 180f, -sprite.transform.localEulerAngles.z);
        }
    }
    void Normal()
    {
        m_Velocity.y = -1f;
        //Rotate towards ground
        RaycastHit hit;
        ray = new Ray(sprite.transform.position, Vector3.down);

        if (Physics.Raycast(ray, out hit))
        {
            Quaternion target = Quaternion.EulerAngles(hit.normal);
            sprite.transform.rotation = Quaternion.SlerpUnclamped(sprite.transform.rotation, target, Time.deltaTime * 20f);
            sprite.transform.rotation = Quaternion.EulerAngles(0f, 0f, sprite.transform.rotation.z * 2f);
        }
        //Movement
        float CurrentAcceleration;
        float CurrentMaxSpeed;
        if (Dash)
        {
            if(Mathf.Abs(m_Velocity.x) > m_MaxSpeed)
            {
                CurrentAcceleration = HorizontalInput * m_DashAcceleration;
            }
            else
            {
                CurrentAcceleration = HorizontalInput * m_Acceleration;
            }
            CurrentMaxSpeed = HorizontalInput * m_MaxDashSpeed;
        }
        else
        {
            CurrentAcceleration = HorizontalInput * m_Acceleration;
            CurrentMaxSpeed = HorizontalInput * m_MaxSpeed;
        }
        
        //Slow down if over the max
        if (Mathf.Abs(m_Velocity.x) > Mathf.Abs(CurrentMaxSpeed))
        {
            m_Velocity.x -= m_Friction * Mathf.Sign(m_Velocity.x);
        }
        //Accelerate
        if (Mathf.Abs(m_Velocity.x) < Mathf.Abs(CurrentMaxSpeed))
        {
            m_Velocity.x += CurrentAcceleration;
        }


        //If character is grounded
        if (m_Grounded)
        {

            //Apply Friction
            if (Mathf.Abs(HorizontalInput) < 0.2f)
            {
                if (Mathf.Abs(m_Velocity.x) > 0f)
                {
                    m_Velocity.x -= Mathf.Sign(m_Velocity.x) * m_Friction;
                }
                if (Mathf.Abs(m_Velocity.x) <= m_Friction)
                {
                    m_Velocity.x = 0f;
                }
            }
            //JUMP
            if (Jump)
            {
                m_Velocity.y = m_JumpForce;
                state = states.air;
            }
        }
        else
        {
            state = states.air;
            m_Velocity.y = 0f;
        }
        //Attack
        if (ForwardAttackButton)
        {
            state = states.forwardAttack;
        }

        //Sprite

        if (HorizontalInput > 0.2f && m_Velocity.x > 0f)
        {
            sprite.flipX = true;
        }
        if (HorizontalInput < -0.2f && m_Velocity.x < 0f)
        {
            sprite.flipX = false;
        }

        //m_Hitboxes.localRotation = sprite.transform.localRotation;
    }
    void Air()
    {
        //Sprite
        Quaternion target = Quaternion.EulerAngles(0f,0f,0f);
        sprite.transform.rotation = Quaternion.SlerpUnclamped(sprite.transform.rotation, target, Time.deltaTime * 2);
        sprite.transform.rotation = Quaternion.EulerAngles(0f, 0f, sprite.transform.rotation.z * 2f);

        //Move
        if (!m_WallRight && !m_WallLeft)
        {
            m_CanJumpOnWall = true;
        }

        float CurrentAcceleration = HorizontalInput * m_AirAcceleration;
        float CurrentMaxSpeed = HorizontalInput * m_MaxSpeed;

        //Accelerate
        if (HorizontalInput < 0)
        {
            if (m_Velocity.x > CurrentMaxSpeed)
            {
                m_Velocity.x += CurrentAcceleration;
            }
        }
        else
        if (HorizontalInput > 0)
        {
            if (m_Velocity.x < CurrentMaxSpeed)
            {
                m_Velocity.x += CurrentAcceleration;
            }
        }
        //Hit ceiling
        if (m_Ceiling && m_Velocity.y > 0f)
        {
            m_Velocity.y = 0f;
        }
        //Gravity

        if (JumpIsDown)
        {
            m_Velocity.y -= m_JumpGravity;
        }
        else
        {
            m_Velocity.y -= m_NormalGravity;
        }

        if (Duck)
        {
            m_Velocity.y -= m_FallGravity;
        }
        if (m_Velocity.y < -m_MaxGravity)
        {
            m_Velocity.y = -m_MaxGravity;
        }
        //Hit the ground
        if (m_Grounded && m_Velocity.y < 0f)
        {
            state = states.normal;
        }
        //Check for wall and stuff
        if (!DuckIsDown && ((Mathf.Abs(HorizontalInput) > 0.2f && ((HorizontalInput > 0f && m_WallRight) || (HorizontalInput < 0f && m_WallLeft))) && m_CanJumpOnWall))
        {
            frame = 0;
            m_CanRunUpWall = false;
            state = states.wall;
            m_Velocity.x = 0f;
            if (m_Velocity.y < 0f)
            {
                m_Velocity.y = 0f;
            }
        }
    }
    void Wall()
    {
        //Jump off
        float stupidFactor = 20f;
        if (m_WallRight)
        {
            if (HorizontalInput < -0.2f)
            {
                m_Velocity.x -= m_Acceleration / stupidFactor;
            }
            if (Jump)
            {

                m_CanJumpOnWall = false;
                state = states.air;
                m_Velocity.y = m_JumpForce * 1.5f;
                m_Velocity.x = -m_JumpForce * 1.5f;
            }
        }
        else if (m_WallLeft)
        {
            if (HorizontalInput > 0.2f)
            {
                m_Velocity.x += m_Acceleration / stupidFactor;
            }

            if (Jump)
            {

                m_CanJumpOnWall = false;
                state = states.air;
                m_Velocity.y = m_JumpForce * 1.5f;
                m_Velocity.x = m_JumpForce * 1.5f;
            }
        }
        //Fall off
        if (Duck)
        {
            state = states.air;
        }
        //Run up

        if (Mathf.Abs(HorizontalInput) < 0.8f)
        {
            m_CanRunUpWall = true;
        }
        if ((Mathf.Abs(HorizontalInput) > 0.8f && ((HorizontalInput > 0f && m_WallRight) || (HorizontalInput < 0f && m_WallLeft))) && m_CanRunUpWall)
        {
            RaycastHit hit;
            ray = new Ray(m_CeilingCheck.position, Vector3.right * Mathf.Sign(HorizontalInput));

            if (Physics.Raycast(ray, out hit))
            {
                m_Velocity = (Quaternion.AngleAxis(-85 * Mathf.Sign(HorizontalInput), Vector3.forward) * hit.normal) * m_JumpForce * 1.5f;
            }

            m_CanRunUpWall = false;
        }

        //Gravity
        if (true)
        {
            RaycastHit hit;
            if (m_WallLeft)
            {
                ray = new Ray(m_CeilingCheck.position, Vector3.right * -1);
            }
            if (m_WallRight)
            {
                ray = new Ray(m_CeilingCheck.position, Vector3.right * 1);
            }

            if (Physics.Raycast(ray, out hit))
            {
                if (m_WallLeft)
                {
                    m_Velocity.y -= (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).y * m_WallGravity;
                    m_Velocity.x -= (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).x * m_WallGravity;

                    if (m_Velocity.y < (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).y * -m_MaxWallGravity)
                    {
                        m_Velocity.y = (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).y * -m_MaxWallGravity;
                    }
                    if (m_Velocity.x < (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).x * -m_MaxWallGravity)
                    {
                        m_Velocity.x = (Quaternion.AngleAxis(90, Vector3.forward) * hit.normal).x * -m_MaxWallGravity;
                    }

                }
                else if (m_WallRight)
                {
                    m_Velocity.y -= (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).y * m_WallGravity;
                    m_Velocity.x -= (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).x * m_WallGravity;

                    if (m_Velocity.y < (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).y * -m_MaxWallGravity)
                    {
                        m_Velocity.y = (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).y * -m_MaxWallGravity;
                    }
                    if (Mathf.Abs(HorizontalInput) < 0.2f && m_Velocity.x < (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).x * -m_MaxWallGravity)
                    {
                        m_Velocity.x = (Quaternion.AngleAxis(-90, Vector3.forward) * hit.normal).x * -m_MaxWallGravity;
                    }
                }
            }
        }
        //if (m_Velocity.y < -m_MaxWallGravity)
        //{
        //    m_Velocity.y = -m_MaxWallGravity;
        //}
        //Hit ceiling
        if (m_Ceiling && m_Velocity.y > 0f)
        {
            m_Velocity.y = 0f;
        }
        //Change state
        if (!m_WallLeft && !m_WallRight)
        {
            state = states.air;
        }
        if (m_Grounded)
        {
            state = states.normal;
        }

        //Sprite

        if (m_WallRight)
        {
            sprite.flipX = true;
        }
        if (m_WallLeft)
        {
            sprite.flipX = false;
        }
        //Rotate towards ground
        RaycastHit spritehit;
        if (m_WallRight)
        {
            ray = new Ray(sprite.transform.position, Vector3.right);
        }
        if (m_WallLeft)
        {
            ray = new Ray(sprite.transform.position, Vector3.right*-1);
        }
        

        if (Physics.Raycast(ray, out spritehit))
        {
            Quaternion target = Quaternion.AngleAxis(90f,spritehit.normal);
            sprite.transform.rotation = Quaternion.SlerpUnclamped(sprite.transform.rotation, target, Time.deltaTime * 30f);
            sprite.transform.rotation = Quaternion.EulerAngles(0f, 0f, sprite.transform.rotation.z * 2f);
        }

    }
    void ForwardAttack(int StartupFrames, int AttackFrames, int CooldownFrames)
    {
        frame++;
        
        if(frame <= StartupFrames)
        {
            if (m_Grounded)
            {
                if (Mathf.Abs(m_Velocity.x) > 0f)
                {
                    m_Velocity.x -= m_Friction * Mathf.Sign(m_Velocity.x);
                }
                if (Mathf.Abs(m_Velocity.x) <= m_Friction)
                {
                    m_Velocity.x = 0f;
                }
            }
        }

        if(frame > StartupFrames && frame < StartupFrames+AttackFrames)
        {
            //Move
            if (sprite.flipX)
            {
                m_Velocity.x = m_MaxDashSpeed*2;
            }else
            {
                m_Velocity.x = -m_MaxDashSpeed*2;
            }

            //Hitbox magic
            Collider col = attackHitboxes[0];
            Collider[] cols = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation, m_WhatIsGround);
            foreach (Collider c in cols)
            {
                if (c.transform.parent.parent == transform)
                    continue;

                DamageInfo damageInfo = new DamageInfo();
                damageInfo.direction = new Vector3(10f, 10f, 0f);
                damageInfo.damage = 10f;
                damageInfo.DebugLog = "It Works";

                c.SendMessageUpwards("TakeDamage", damageInfo);
                Debug.Log(c.name);
            }
        }

        if(frame > StartupFrames + AttackFrames)
        {
            if (m_Grounded)
            {
                if (Mathf.Abs(m_Velocity.x) > 0f)
                {
                    m_Velocity.x -= m_Friction * Mathf.Sign(m_Velocity.x);
                }
                if (Mathf.Abs(m_Velocity.x) <= m_Friction)
                {
                    m_Velocity.x = 0f;
                }
            }
        }

        //Change state
        if(frame > StartupFrames + CooldownFrames + AttackFrames)
        {
            if (m_Grounded)
            {
                state = states.normal;
            }else
            {
                state = states.air;
            }
        }
    }

}
