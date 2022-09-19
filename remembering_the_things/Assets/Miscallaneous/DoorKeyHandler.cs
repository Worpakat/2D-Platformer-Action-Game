using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKeyHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.HaveKey = true;
            Destroy(gameObject);
        }
    }
}
