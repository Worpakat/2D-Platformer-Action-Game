using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkellyArcher : SkellyBase
{
    enum SkeletonBeheviour
    {
        Watching,
        Attacking
    }

    private SkeletonBeheviour currentBehaviour = SkeletonBeheviour.Watching;

    private Transform playerTr;
    private Collider2D playerCollider = null;
    [SerializeField] private LayerMask playerLayer;

    private float lastDirection = -1f;

    [SerializeField] private float detectionRange = 0f;
    [SerializeField] private float attackRate = 1f; //1sn de 1 kere
    private bool canAttack = true;


    public GameObject arrowPrefab;
    public Transform arrowPoint;
    [SerializeField] private float arrowSpeed = 10f;

    protected override void Awake()
    {
        base.Awake();

        playerTr = GameObject.FindGameObjectWithTag("Player").transform;
        arrowPoint = transform.GetChild(0);
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
            case SkeletonBeheviour.Watching: //Watching

                if (CheckPlayerInRange())
                {
                    currentBehaviour = SkeletonBeheviour.Attacking;
                }

                break;

            case SkeletonBeheviour.Attacking: //Attacking

                CheckAndFlip();
                //StartCoroutine(Attack()); //!!FixedUpdate() e bak !!

                if (!CheckPlayerInRange())
                {
                    currentBehaviour = SkeletonBeheviour.Watching;
                }

                break;

            default:
                currentBehaviour = SkeletonBeheviour.Watching;

                break;
        }
    }
    private void FixedUpdate()
    {
        if(currentBehaviour == SkeletonBeheviour.Attacking)
        {
            StartCoroutine(Attack());
        }
    }
    private bool CheckPlayerInRange()
    {
        //Player'�n sald�r� menzilinde olup olmad���n� return eder.
        playerCollider = Physics2D.OverlapCircle(enemyCollider.bounds.center, detectionRange, playerLayer);
        
        CheckAndFlip(); //!!D�KKAT:Bu daha verimli bi �ekilde yap�labilir gibi, bi ara kontrol et d�zenle daha.

        if(playerCollider != null)
        {
            /*!!Alana girdi�ini farkederse archer direk player'a do�ru raycastliyor.
             E�er raycast ilk �arpt��� player olursa player'� g�rm�� olacak
            yani arada bi�i varsa g�remez ve s�kmaz*/
            Vector2 direction = playerCollider.bounds.center - arrowPoint.position;
            RaycastHit2D hitObj = Physics2D.Raycast(arrowPoint.position, direction);

            return hitObj.collider.CompareTag("Player"); //�lk saptad��� Player sa true return eder.
        }

        return false; //saptama alan�nda de�ilse direk false d�nd�rmeli
    }
    private void CheckAndFlip()
    {
        if (CheckHaveToFlip())
        {
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            lastDirection = Mathf.Sign(-transform.localScale.x);
        }
        
    }
    private bool CheckHaveToFlip()
    { 
        if((transform.position.x > playerTr.position.x && lastDirection == 1) || (transform.position.x < playerTr.position.x && lastDirection == -1)) 
        {
            return true;
        }
        //Else
        return false;
    }
    private IEnumerator Attack()
    {
        if (canAttack)
        {
            enemyAnimator.SetInteger("animationNum", 1);
            float animationLength = enemyAnimator.GetCurrentAnimatorClipInfo(0).Length;
            animationLength *= 0.9f; //%90 �n� ald�m ��nk� biraz ge� kal�yor

            Vector2 firstDetectionPoint = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.center.y + playerCollider.bounds.size.y / 3);
        
            StartCoroutine(ShootArrow(animationLength, firstDetectionPoint));

            canAttack = false;
            yield return new WaitForSeconds(1f / attackRate);
            canAttack = true;
        }
        else
        {
            enemyAnimator.SetInteger("animationNum", 0);
            yield return null;
        }

    }

    private IEnumerator ShootArrow(float animationLength, Vector2 firstDetectionPoint)
    {
        yield return new WaitForSeconds(animationLength);

        //arrow shooting
        print("shoot"+canAct);
        if (canAct)
        {
            Vector2 lastDetectionPoint = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.center.y + playerCollider.bounds.size.y / 3);
            Vector2 shootPoint = (firstDetectionPoint + lastDetectionPoint) / 2;
            /*!!�uan oku �ekmeye ba�lad��� andaki ve oku b�rakt��� andaki player positionlar�n�n ortas�n� at�� noktas� olarak belirledim.
             Niyesi �u: Direk hareket ederken dahi tam adama s�k�nca baya zorla��yo gibi b�yle bir hata pay� verdik.
            Ok �ekilirkenki s�rede ne kadar hareket o kadar �ska ihtimali, ne kadar az hareket o kadar isabet ihtimali.*/

            Vector2 arrowPointV2 = arrowPoint.position; //V2 den V3 ��kar�nca hata veriyor

            Vector2 direction = shootPoint - arrowPointV2;

            GameObject arrow = Instantiate(arrowPrefab, arrowPointV2, arrowPrefab.transform.rotation);
            Rigidbody2D arrowRB = arrow.GetComponent<Rigidbody2D>();
            arrowRB.velocity = arrowSpeed * Time.fixedDeltaTime * direction.normalized;

        }
    }


    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(enemyCollider.bounds.center, detectionRange);   //Player Detection circle
        //Vector2 boxPos = new Vector2(transform.position.x - transform.localScale.x, skeletonWarrCollider.bounds.center.y);
        //Gizmos.DrawCube(boxPos, new Vector2(1, 1.5f));
        //Gizmos.DrawRay(arrowPoint.position, publicPlayerCollider.bounds.center - arrowPoint.position);

    }

}
