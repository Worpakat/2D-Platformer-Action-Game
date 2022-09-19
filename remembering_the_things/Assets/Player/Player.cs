using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    private const float ACT_COOLDOWN = 0.5f;
    private const float JUMP_PRESSED_REMEMBER_TIME = 0.2f;

    private bool canAct = true;
    private bool canMove = true;
    
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float jumpSpeed = 7f;

    private Rigidbody2D playerRigidbody;
    //private Collider2D playerCollider;
    public Collider2D playerCollider;

    private Animator playerAnimator;
    private string currentAnimationState;
    //_____Animation Names____
    private const string IDLE_ANIMATION = "HeroKnight_Idle";
    private const string WALK_ANIMATION = "HeroKnight_Run";
    private const string JUMP_ANIMATION = "HeroKnight_Jump";
    private const string FALL_ANIMATION = "HeroKnight_Fall";
    private const string ATTACK_1_ANIMATION = "HeroKnight_Attack1";
    private const string ATTACK_2_ANIMATION = "HeroKnight_Attack2";
    private const string ATTACK_3_ANIMATION = "HeroKnight_Attack3";
    private const string BLOCK_IDLE_ANIMATION = "HeroKnight_BlockIdle";
    private const string BLOCK_ANIMATION = "HeroKnight_Block";
    private const string HURT_ANIMATION = "HeroKnight_Hurt";
    private const string DEATH_ANIMATION = "HeroKnight_Death";

    private string[] attackAnimNames = { ATTACK_1_ANIMATION, ATTACK_2_ANIMATION, ATTACK_3_ANIMATION };

    Dictionary<string, float> animationLenghts = new Dictionary<string, float>();
    

    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private int playerHealth = 100;
    public int PlayerHealth { get { return playerHealth; } set { playerHealth = value; } }
    public Slider healthBar;
    private bool isAlive = true;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField]private float attackRadius = 0;
    [SerializeField]private int attackDamage = 0;
    [SerializeField] private float attackRate = 1.5f;
    public Transform attackPoint;
    private int comboCounter = 0;
    float attackOverTime = 0;
    private float nextAttackTime = 0;

    [SerializeField] private float blockingTime = 0;
    private float blockDirection = 0;

    //===========================000=========================///

    private bool haveKey = false;
    public bool HaveKey { get { return haveKey; } set { haveKey = value; } }


    //==========INPUT CHECKERS=============//
    private float walkInput = 0f;
    private bool isWalking = false;
    private bool jumpPressed = false;
    private bool attackPressed = false; 
    private bool isAttacking = false;
    private bool dashPressed = false;
    private bool isDashing = false;
    private bool blockPressed = false;
    private bool isBlocking = false;

    private bool isGrounded; //This means also bool isJumping
    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        //playerCollider = GetComponent<Collider2D>();
        playerAnimator = GetComponent<Animator>();

        //attackPoint = transform.GetChild(0);

        foreach(AnimationClip animation in playerAnimator.runtimeAnimatorController.animationClips)
        {
            animationLenghts.Add(animation.name, animation.length);
        }
    

    }

    private void Update()
    {
        if (!isAlive)
        {
            playerRigidbody.velocity = Vector2.zero;
            StartCoroutine(RestartLevel());
            return;
        }
        if (!canAct)
        {
            StartCoroutine(WaitUntilCanAct());
            return;
        }

        TakeWalkInput();
        TakeJumpInput();
        TakeAttackInput();
        TakeDashInput();
        TakeBlockInput();

        isGrounded = IsGrounded();

        //not doing anything physic based in this methods
        ComboAttack(); 
        Dash();
        Block();

    }
    
    private void FixedUpdate()
    {
        if (!isAlive || !canAct)
            return;

        Walk();
        Jump();
    }

    private void ChangeAnimationState(string animationState)
    {
        //Changes current animation to want.
        if (currentAnimationState == animationState) return; //Ayný animasyonu baþtan almasýn diye böyle bir kod koyduk

        if (currentAnimationState == BLOCK_ANIMATION && animationState == BLOCK_IDLE_ANIMATION) return; //Ýdareten bi satýr:
        //Block animationdan Block Idle a dönmesini engelleyip Block animation devam etmesi için . Deðiþecek

        playerAnimator.Play(animationState);

        currentAnimationState = animationState;
    }

    private void TakeWalkInput()
    {
        walkInput = Input.GetAxisRaw("Horizontal");
    }
    private void TakeJumpInput()
    {
        jumpPressed =jumpPressed || Input.GetKeyDown(KeyCode.Space);
        //https://www.reddit.com/r/Unity3D/comments/54bre4/input_fixed_update_lost_input/
        //Hayat kurtarýcý kýsým: "ProgrammingPigeon" ýn yorumu, böyle yapmadan önce input loss oluyodu.
    }
    private void TakeAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            comboCounter++;
            isAttacking = true;
            attackPressed = true;
        }
    }
    private void TakeDashInput()
    {
        dashPressed = Input.GetKeyDown(KeyCode.LeftControl) && walkInput != 0 && !isDashing;
        if (dashPressed)
        {
            dashDirection = walkInput;
        }
    }
    private void TakeBlockInput()
    {
        blockPressed = Input.GetKeyDown(KeyCode.Z)  && !isDashing;
        if (blockPressed)
        {
            blockDirection = Mathf.Sign(transform.localScale.x);
        }
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private IEnumerator WaitUntilCanAct()
    {
        playerRigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(ACT_COOLDOWN);
        canAct = true;
    }

    private void Walk()
    {
        if (!canMove || isAttacking || isBlocking)
            return;

        playerRigidbody.velocity = new Vector2(walkInput * movementSpeed, playerRigidbody.velocity.y);
        Flip(walkInput);

        if (isGrounded)
        {
            if (walkInput == 0)
            {
                ChangeAnimationState(IDLE_ANIMATION);
                isWalking = false;
            }
            else
            {
                ChangeAnimationState(WALK_ANIMATION);
                isWalking = true;
            }

        }
    }
    private void Flip(float input)
    {
        if(input > 0.01f )
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }
        else if (input < -0.01f)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }

    }

    private float passTimeFromLastJumpPress = 0f;
    private void Jump()
    {
        passTimeFromLastJumpPress -= Time.fixedDeltaTime;

        if (jumpPressed)
        {
            jumpPressed = false;
            passTimeFromLastJumpPress = JUMP_PRESSED_REMEMBER_TIME;
        }

        if (passTimeFromLastJumpPress > 0 && isGrounded)
        {
            playerRigidbody.AddForce(Vector2.up * jumpSpeed);
        }

        if (!isGrounded && !isAttacking && !isBlocking) //when jumping...
        {
            if (playerRigidbody.velocity.y > 0) //if vertical velocity toward up..
            {
                ChangeAnimationState(JUMP_ANIMATION);
            }
            else if(playerRigidbody.velocity.y < 0)
            {
                ChangeAnimationState(FALL_ANIMATION);
            }
        }
    }
    private bool IsGrounded()
    {
        /*Playerdan yere doðru raycast leyip altýnda biþi varmý kontrol eden method*/

        float rayLength = 0.025f;
        Vector2 origin1 = new Vector2(playerCollider.bounds.min.x + 0.075f, playerCollider.bounds.min.y - 0.001f);
        Vector2 origin2 = new Vector2(playerCollider.bounds.max.x - 0.075f, playerCollider.bounds.min.y - 0.001f);

        RaycastHit2D raycastHitObj1 = Physics2D.Raycast(origin1, Vector2.down, rayLength);
        RaycastHit2D raycastHitObj2 = Physics2D.Raycast(origin2, Vector2.down, rayLength);

        return raycastHitObj1.collider != null || raycastHitObj2.collider != null;
    }

    private void ComboAttack()
    {
        if (isAttacking)
        {
            if (attackPressed)
            {
                playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);

                if (comboCounter == 1)
                {
                    Attack(ATTACK_1_ANIMATION);
                }

                if (comboCounter >= 2 && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(ATTACK_1_ANIMATION))
                {
                    Attack(ATTACK_2_ANIMATION);
                }

                if (comboCounter >= 3 && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f && playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(ATTACK_2_ANIMATION))
                {
                    Attack(ATTACK_3_ANIMATION);
                }
            }

        }

        if (Time.time >= attackOverTime)
        {
            isAttacking = false;
            comboCounter = 0;
        }


        //if (Time.time >= nextAttackTime) //Bir önceki atakla arasýna yeterince süre koyduk ki spamlayamasýn
        //{
        //    ChangeAnimationState(ATTACK_1_ANIMATION);
        //    float waitTime = playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        //    waitTime /= 6; //%50 sini aldým çünkü tam vurma anýna denk gelsin

        //    StartCoroutine(ApplyDamageEnemies(waitTime));

        //    nextAttackTime = Time.time + (1f / attackRate);

        //    float waitSecs = playerAnimator.GetCurrentAnimatorClipInfo(0).Length;  //!!!__SALDIRIRKEN KARAKTER DURSUN ÝSTERSEN
        //    StartCoroutine(StopUntilEndAttack(waitSecs * 0.45f));                   //BU 2 SATIRI ETKÝNLEÞTÝR               
        //}

    }
    private void Attack(string attackName)
    {
        attackPressed = false;
        ChangeAnimationState(attackName);
        attackOverTime = Time.time + animationLenghts[attackName];
        StartCoroutine(ApplyDamageEnemies(animationLenghts[attackName] / 2));
    }
    private IEnumerator ApplyDamageEnemies(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);

        Collider2D[] beenHitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemy in beenHitEnemies)
        {
            SkellyBase enemyScript = enemy.GetComponent<SkellyBase>();
            enemyScript.DamageToEnemy(attackDamage);
        }

    }


    #region DASH METHODS AND FIELDS
    [SerializeField] private float beginningDashSpeed = 0f;
    [SerializeField] private float dashProcessingTime = 0f;
    [SerializeField] private float dashCooldown = 0f;
    private float dashDirection = 0;
    private float currentDashSpeed = 0;
    private bool canDash = true;
    private void Dash()
    {
        if (!canMove)
            return;

        if (dashPressed && canDash)
        {
            canDash = false;
            dashPressed = false;
            isDashing = true;
            currentDashSpeed = beginningDashSpeed;
            
            StartCoroutine(WaitForDashCooldown());
            StartCoroutine(WaitForDashEnding());
        }
        if (isDashing) //dodging true olduðu sürece buna girecekki yavaþlatsýn
        {
            DashSlide();
        }
    }
    private void DashSlide()
    {
        playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
        transform.position += new Vector3(dashDirection, 0) * currentDashSpeed * Time.deltaTime;
    }
    private IEnumerator WaitForDashEnding()
    {
        yield return new WaitForSeconds(dashProcessingTime);
        isDashing = false;
    }
    private IEnumerator WaitForDashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    #endregion

    private void Block()
    {
        if (blockPressed && !isDashing)
        {
            //belirtilen süre kadar block idle anim oynat
            
            blockPressed = false;
            isBlocking = true;
            
            playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);

            StartCoroutine(EndBlockingWithTime());
        }
        if (isBlocking)
        {
            ChangeAnimationState(BLOCK_IDLE_ANIMATION);
        }
    }
    private IEnumerator EndBlockingWithTime()
    {
        yield return new WaitForSeconds(blockingTime);
        isBlocking = false;
    }

    public void IncreaseHealth(int increaseAmount)
    {
        int current = playerHealth + increaseAmount;
        current = Mathf.Clamp(current, 0, 100);
        SetHealth(current);
    }
    private void SetHealth(int currentHealth)
    {
        playerHealth = currentHealth;
        healthBar.value = currentHealth;
    }
    public void DamageToPlayer(int damage, bool _canAct, float attackDirection)
    {
        //if player blocking return, he's not gonna take damage.
        if (isBlocking && blockDirection != attackDirection)
        {
            print("blocked");
            ChangeAnimationState(BLOCK_ANIMATION);
            return;
        }


        //_______Apply damage to player
        
        SetHealth(playerHealth - damage);

        if(playerHealth <= 0)
        {
            KillPlayer();
        }
        else
        {
            //damage animation
            canAct = _canAct;
            ChangeAnimationState(HURT_ANIMATION);
        }
    }
    private void KillPlayer()
    {
        print("DEATH");
        isAlive = false;
        playerRigidbody.gravityScale = 0;
        playerRigidbody.velocity = Vector2.zero;
        playerCollider.enabled = false;
        ChangeAnimationState(DEATH_ANIMATION);
    }

    private void OnDrawGizmos()
    {
        //ISGROUNDED RAY CASTS
        //Vector2 origin1 = new Vector2(playerCollider.bounds.min.x + 0.075f, playerCollider.bounds.min.y - 0.001f);
        //Vector2 origin2 = new Vector2(playerCollider.bounds.max.x - 0.075f, playerCollider.bounds.min.y - 0.001f);

        //Gizmos.DrawRay(origin1, Vector2.down);
        //Gizmos.DrawRay(origin2, Vector2.down);

        //ATTACK POINT CIRCLE CAST
        //Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }


    //=========UNUSED METHOD & FIELDS==========//

    ////private bool isOnWall;
    ////private bool isWallSliding;
    [SerializeField] private float wallJumpSpeedX = 0f;
    [SerializeField] private float wallJumpSpeedY = 0f;
    private bool IsOnWall() //Ýstediðim gibi olmadý bende koymadým, belki sonra bir daha bakarým.
    {
        /*Playerdan box raycast leyip herhangi bi duvara temas edip etmediðine bakan method*/

        Vector2 direction = new Vector2(transform.localScale.x, 0);
        float rayLength = 0.5f;

        RaycastHit2D raycastHitWall = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, direction, rayLength, wallLayer);
        
        return raycastHitWall.collider != null;
    }
    private void WallJump()
    {
        playerRigidbody.velocity = new Vector2(wallJumpSpeedX * -Mathf.Sign(walkInput), wallJumpSpeedY);
    }
}


//void Update()  //DEPRECATED!!
//{
//    if (!isAlive)
//    {
//        playerRigidbody.velocity = Vector2.zero;
//        StartCoroutine(RestartLevel());
//        return;
//    }

//    if (!canAct)
//    {
//        StartCoroutine(WaitUntilCanAct());
//        return;
//    }

//    Move();

//    Attack();

//    Jump();

//    Dash();

//}

//void Move()  //DEPRECATED!!
//{
//    if (!canMove)
//        return;

//    walkInput = Input.GetAxisRaw("Horizontal");
//    playerRigidbody.velocity = new Vector2(walkInput * movementSpeed, playerRigidbody.velocity.y);
//    Flip(walkInput);
//    if (walkInput == 0)
//    {
//        playerAnimator.SetInteger("animationNum", 0);
//    }
//    else
//    {
//        playerAnimator.SetInteger("animationNum", 1);
//    }
//}

//private void Attack()    //DEPRECATED
//{
//    if (attackPressed && !isDashing) //dodgelarken saldýrmayý kapatcaksan "&& !dodging" ekle condition'a 
//    {
//        if (Time.time >= nextAttackTime) //Bir önceki atakla arasýna yeterince süre koyduk ki spamlayamasýn
//        {

//            ChangeAnimationState(ATTACK_1_ANIMATION);
//            float waitTime = playerAnimator.GetCurrentAnimatorStateInfo(0).length;
//            waitTime /= 6; //%50 sini aldým çünkü tam vurma anýna denk gelsin

//            StartCoroutine(ApplyDamageEnemies(waitTime));

//            nextAttackTime = Time.time + (1f / attackRate);

//            float waitSecs = playerAnimator.GetCurrentAnimatorClipInfo(0).Length;  //!!!__SALDIRIRKEN KARAKTER DURSUN ÝSTERSEN
//            StartCoroutine(StopUntilEndAttack(waitSecs * 0.45f));                   //BU 2 SATIRI ETKÝNLEÞTÝR               

//            //attackPressed = false;

//        }

//    }
//}

//private IEnumerator StopUntilEndAttack(float seconds) //Saldýrýrken yürümeyi durdurmak için !!DEPRECATED
//{
//    canMove = false;

//    playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);

//    yield return new WaitForSeconds(seconds);

//    canMove = true;
//    attackPressed = false;

//}

