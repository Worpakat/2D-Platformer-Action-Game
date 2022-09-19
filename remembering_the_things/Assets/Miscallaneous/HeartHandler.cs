using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartHandler : MonoBehaviour
{
    [SerializeField] private int healthIncreaseAmount = 0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.transform.GetComponent<Player>();
            playerScript.IncreaseHealth(healthIncreaseAmount);
            Destroy(gameObject);
        }
    }
}
