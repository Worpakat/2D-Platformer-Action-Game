using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField] int damage = 0;
    [SerializeField] int forceAmount = 0;
    [SerializeField] LayerMask playerLayer;
   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Collider2D playerCol = collision.gameObject.GetComponent<Collider2D>();
            Player playerScript = playerCol.GetComponent<Player>();

            float attackDirection = Mathf.Sign(playerCol.transform.localScale.x); //For applying damage while player blocking, we said here it attacking back everytime
            playerScript.DamageToPlayer(damage, true, attackDirection); //apply damage to player
            
            //Apply force to player from touchdamage gameobject center
            Rigidbody2D playerRB = playerCol.GetComponent<Rigidbody2D>();
            playerRB.velocity = Vector2.zero;
            playerRB.AddForce((playerCol.bounds.center - transform.position).normalized * forceAmount); 
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Collider2D playerCol = collision.gameObject.GetComponent<Collider2D>();
            Rigidbody2D playerRB = playerCol.GetComponent<Rigidbody2D>();
            playerRB.AddForce((playerCol.bounds.center - transform.position).normalized * forceAmount*Time.fixedDeltaTime);
        }
    }
}
