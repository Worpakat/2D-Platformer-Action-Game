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
        //Player'ýn saldýrý menzilinde olup olmadýðýný return eder.
        playerCollider = Physics2D.OverlapCircle(enemyCollider.bounds.center, detectionRange, playerLayer);
        
        CheckAndFlip(); //!!DÝKKAT:Bu daha verimli bi þekilde yapýlabilir gibi, bi ara kontrol et düzenle daha.

        if(playerCollider != null)
        {
            /*!!Alana girdiðini farkederse archer direk player'a doðru raycastliyor.
             Eðer raycast ilk çarptýðý player olursa player'ý görmüþ olacak
            yani arada biþi varsa göremez ve sýkmaz*/
            Vector2 direction = playerCollider.bounds.center - arrowPoint.position;
            RaycastHit2D hitObj = Physics2D.Raycast(arrowPoint.position, direction);

            return hitObj.collider.CompareTag("Player"); //Ýlk saptadýðý Player sa true return eder.
        }

        return false; //saptama alanýnda deðilse direk false döndürmeli
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
            animationLength *= 0.9f; //%90 ýný aldým çünkü biraz geç kalýyor

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
            /*!!Þuan oku çekmeye baþladýðý andaki ve oku býraktýðý andaki player positionlarýnýn ortasýný atýþ noktasý olarak belirledim.
             Niyesi þu: Direk hareket ederken dahi tam adama sýkýnca baya zorlaþýyo gibi böyle bir hata payý verdik.
            Ok çekilirkenki sürede ne kadar hareket o kadar ýska ihtimali, ne kadar az hareket o kadar isabet ihtimali.*/

            Vector2 arrowPointV2 = arrowPoint.position; //V2 den V3 çýkarýnca hata veriyor

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
