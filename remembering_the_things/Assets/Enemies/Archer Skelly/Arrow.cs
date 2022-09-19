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
            float angle = Mathf.Atan2(arrowRB.velocity.y, arrowRB.velocity.x) * Mathf.Rad2Deg; //Atan2 methodu arc tan �n� alarak y ve x in aras�ndaki a��y� hesapl�yor.
            //Radius cinsinden. Mathf.Rad2Deg ise bu radius a��s�n� derece tipine �evirmeye yarayan constant.
            transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward); //hesaplanan a�� kadar oku 'z' ekseninde �evirdik.
            //Yani dikey ve yatay h�z� kullanarak birle�im vekt�r�n�n a��s�n hesaplay�p oku o a��ya �evirdik. E�ik at�� i�te.
        }
        

    }
    private void OnCollisionEnter2D(Collision2D collision) //Collision oldu�unda olacak olanlar�n yap�laca�� method. Bu event'e ba�l� zaten.
    {
        hasHit = true;
        arrowRB.velocity = Vector2.zero;
        arrowRB.isKinematic = true; //saplan�p kalmas� i�in

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
