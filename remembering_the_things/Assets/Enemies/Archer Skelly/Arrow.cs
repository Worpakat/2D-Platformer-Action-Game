using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool hasHit = false;
    private Rigidbody2D arrowRB;
    private Collider2D arrowCol;
    [SerializeField] private int arrowDamage = 7;
    void Awake()
    {
        arrowRB = GetComponent<Rigidbody2D>();
        arrowCol = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasHit)
        {
            float angle = Mathf.Atan2(arrowRB.velocity.y, arrowRB.velocity.x) * Mathf.Rad2Deg; //Atan2 methodu arc tan ýný alarak y ve x in arasýndaki açýyý hesaplýyor.
            //Radius cinsinden. Mathf.Rad2Deg ise bu radius açýsýný derece tipine çevirmeye yarayan constant.
            transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward); //hesaplanan açý kadar oku 'z' ekseninde çevirdik.
            //Yani dikey ve yatay hýzý kullanarak birleþim vektörünün açýsýn hesaplayýp oku o açýya çevirdik. Eðik atýþ iþte.
        }
        

    }
    private void OnCollisionEnter2D(Collision2D collision) //Collision olduðunda olacak olanlarýn yapýlacaðý method. Bu event'e baðlý zaten.
    {
        hasHit = true;
        arrowRB.velocity = Vector2.zero;
        arrowRB.isKinematic = true; //saplanýp kalmasý için

        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            float attackDirection = Mathf.Sign(playerScript.transform.position.x - transform.position.x);
            playerScript.DamageToPlayer(arrowDamage, false, attackDirection);
            Destroy(gameObject);
        }
        else
        {
            arrowCol.enabled = false;
            StartCoroutine(WaitAndDestroy());
        }
    }
    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
