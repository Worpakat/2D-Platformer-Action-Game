using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SkeletonBeheviour
{
    Patroling,
    Following,
    Attacking
}
public class SkellyWarrior : SkellyBase
{
    
    private SkeletonBeheviour currentBehaviour = SkeletonBeheviour.Patroling;

    [SerializeField] float patrolSpeed = 0f;
    [SerializeField] float walkSpeed = 0f;
    private float lastDirection = -1f;

    private Transform positionCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    private bool canPatrol = true;

    private GameObject playerObj;
    private Player playerScript;
    [SerializeField] private LayerMask playerLayer;
    
    [SerializeField] private float detectionRange = 0f;
    [SerializeField] private int attackDamage = 10;
    private readonly float attackRate = 1f; //1sn de 1 kere
    private float nextAttackTime = 0;

    protected override void Awake()
    {
        base.Awake();

        positionCheckPoint = transform.GetChild(0);
        
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerScript = playerObj.GetComponent<Player>();
    }

    
    protected override void Update()
    {
        if (!isAlive)
            return;

        base.Update();
        
        if (!canAct)
            return;

        switch (currentBehaviour)
        {
            case SkeletonBeheviour.Patroling: //Patrolling

                Patrol();

                if (CheckPlayerInRange())
                {
                    currentBehaviour = SkeletonBeheviour.Following;
                }

                break;

            case SkeletonBeheviour.Following:  //Following 
                
                if (CheckPlayerInAttackRange())
                {
                    currentBehaviour = SkeletonBeheviour.Attacking;
                }
                else if (CheckPlayerInRange())
                {
                    MoveToward(playerObj.transform.position);
                }
                else
                {
                    currentBehaviour = SkeletonBeheviour.Patroling;
                }

                break;
            
            case SkeletonBeheviour.Attacking: //Attacking

                AttackPlayer();

                if (CheckPlayerInRange()) 
                {
                    currentBehaviour = SkeletonBeheviour.Following;
                }
                else
                {
                    currentBehaviour = SkeletonBeheviour.Patroling;
                }

                break;

            default:
                currentBehaviour = SkeletonBeheviour.Patroling;

                break;
        }
    }

    private void MoveToward(Vector2 target)
    {
        enemyAnimator.SetInteger("animationNum", 1);

        float direction = Mathf.Sign(target.x - enemyRB.position.x);
        enemyRB.velocity = new Vector2(walkSpeed * direction, enemyRB.velocity.y);
        
        lastDirection = direction;
        MoveFlip(direction);
    }
    private void MoveFlip(float direction)
    {
        if (Mathf.Sign(transform.localScale.x) == direction)
        {
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }

    private bool CheckPlayerInRange()
    {
        //Player'ýn saldýrý menzilinde olup olmadýðýný return eder.

        Collider2D playerCollider = Physics2D.OverlapCircle(enemyCollider.bounds.center, detectionRange, playerLayer);

        return playerCollider != null;
    }
    private bool CheckPlayerInAttackRange()
    {
        return IsTouchingLayerObj(playerLayer);
    }
    private void AttackPlayer()
    {
        enemyRB.velocity = Vector2.zero; //Stop and ...
        
        if (Time.time >= nextAttackTime) //Bir önceki atakla arasýna yeterince süre koyduk ki spamlayamasýn
        {
            //Attack
            enemyAnimator.SetInteger("animationNum", 2);

            float damageTime = enemyAnimator.GetCurrentAnimatorStateInfo(0).length * 0.8f;
            StartCoroutine(ApplyDamage(damageTime));

            nextAttackTime = Time.time + (1f / attackRate);
        }
        else
        {
            enemyAnimator.SetInteger("animationNum", 0);
        }

    }
    private IEnumerator ApplyDamage(float damageTime)
    {
        yield return new WaitForSeconds(damageTime);

        if (canAct && CheckPlayerInAttackRange())
        {
            float attackDirection = Mathf.Sign(playerObj.transform.position.x - transform.position.x);
            playerScript.DamageToPlayer(attackDamage, false, attackDirection);
        }
    }

    private void Patrol()
    {
        if (canPatrol)
        {
            if(lastDirection != Mathf.Sign(patrolSpeed)) //Eðer last direction la uyumlu deðilse patrol yönü, onu düzeltiyoruz
            {
                patrolSpeed *= -1;
            }
            enemyRB.velocity = new Vector2(Time.fixedDeltaTime * patrolSpeed, enemyRB.velocity.y);
            enemyAnimator.SetInteger("animationNum", 1);
            lastDirection = Mathf.Sign(patrolSpeed);
        }
        else
        {
            enemyAnimator.SetInteger("animationNum", 0);
        }

        if (CheckHaveToFlip())
        {
            PatrolFlip();
        }
    }
    private bool CheckHaveToFlip()
    {
        Collider2D groundCollider = Physics2D.OverlapCircle(positionCheckPoint.position, 0.1f, groundLayer);

        bool returnBool = IsTouchingLayerObj(groundLayer) || groundCollider == null; //!!!DÝKKAT: havadan düþerken sürekli dönüyor düzelt, diðer flip içinde ayný durum var

        return returnBool;
    }
    private void PatrolFlip()
    {
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        patrolSpeed *= -1;
        lastDirection = Mathf.Sign(patrolSpeed);
    }
    private bool IsTouchingLayerObj(LayerMask touchingObjLayer)
    {
        /*Playerdan box raycast leyip herhangi bi duvara temas edip etmediðine bakan method*/

        Vector2 direction = new Vector2(transform.localScale.x, 0);
        float rayLength = 0.5f;
        Vector2 size = new Vector2(1, 1.5f);
        Vector2 boxPosition = new Vector2(transform.position.x - transform.localScale.x, enemyCollider.bounds.center.y);
        RaycastHit2D raycastHitWall = Physics2D.BoxCast(boxPosition, size, 0, direction, rayLength, touchingObjLayer);

        return raycastHitWall.collider != null;
    }

  


    private void OnDrawGizmos()
    {
        //Vector2 boxPos = new Vector2(transform.position.x - transform.localScale.x, enemyCollider.bounds.center.y);
        //Gizmos.DrawCube(boxPos, new Vector2(1, 1.5f));
    }

}
