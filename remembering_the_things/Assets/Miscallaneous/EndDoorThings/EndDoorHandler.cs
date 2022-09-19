using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDoorHandler : MonoBehaviour
{
    public Sprite upSpriteOpen;
    public Sprite midAndDownSpriteOpen;
    private Transform parentTr;
    private void Start()
    {
        parentTr = transform.parent;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player.HaveKey)
            {
                parentTr.GetChild(0).GetComponent<Collider2D>().enabled = false;

                parentTr.GetChild(0).GetComponent<SpriteRenderer>().sprite = upSpriteOpen;
                parentTr.GetChild(1).GetComponent<SpriteRenderer>().sprite = midAndDownSpriteOpen;
                parentTr.GetChild(2).GetComponent<SpriteRenderer>().sprite = midAndDownSpriteOpen;
            }
            else
            {
                parentTr.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
                parentTr.GetChild(1).GetComponent<SpriteRenderer>().color = Color.red;
                parentTr.GetChild(2).GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (!player.HaveKey)
            {
                parentTr.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                parentTr.GetChild(1).GetComponent<SpriteRenderer>().color = Color.white;
                parentTr.GetChild(2).GetComponent<SpriteRenderer>().color = Color.white;
            }

        }
        
    }
}
