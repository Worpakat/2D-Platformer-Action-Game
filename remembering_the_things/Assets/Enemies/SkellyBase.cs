using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkellyBase : MonoBehaviour
{
    private const float ACT_COOLDOWN = 1f;
    protected bool canAct = true;

    [SerializeField]protected int health = 100;
    
    protected bool isAlive = true;

    protected Animator enemyAnimator;
    protected Collider2D enemyCollider;
    protected Rigidbody2D enemyRB;

    public GameObject damageParticlePrefab;

    protected virtual void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
        //if (!canAct)
        //{
        //    StartCoroutine(WaitUntilCanAct());
        //}
    }

    public void DamageToEnemy(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            KillEnemy();
        }
        else
        {
            //damage animation
            enemyAnimator.SetTrigger("hurtTrigger");
            InstantiateDamageParticle();
            canAct = false; //cooldwon doalana dek biþi yapamayacak
            StartCoroutine(WaitUntilCanAct());
        }
    }
    private void KillEnemy()
    {
        isAlive = false;
        canAct = false;
        enemyCollider.enabled = false;
        enemyRB.gravityScale = 0;
        enemyRB.velocity = Vector2.zero;
        enemyAnimator.SetTrigger("deathTrigger");
        
        InstantiateDamageParticle();

        StartCoroutine(WaitAndDestroyThis());

    }
    private IEnumerator WaitUntilCanAct()
    {
        enemyRB.velocity = Vector2.zero;
        
        yield return new WaitForSeconds(ACT_COOLDOWN);
        
        if(isAlive) 
            canAct = true;
    }

    private void InstantiateDamageParticle()
    {
        Instantiate(damageParticlePrefab, transform.position, damageParticlePrefab.transform.rotation, transform);
        StartCoroutine(WaitAndDestroyParticle());
    }
    private IEnumerator WaitAndDestroyParticle()
    {
        yield return new WaitForSeconds(1f);
        Destroy(transform.GetChild(1).gameObject);
    }
    private IEnumerator WaitAndDestroyThis()
    {
        yield return new WaitForSeconds(4f);
        Destroy(transform.gameObject);
    }
}
