using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private const float CAMERA_Z_POSITION = -15;

    private Transform playerTransform; 
    Vector2 playerPos;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothingValue = 4;

    [SerializeField] private float downPanning = 5f;
    [SerializeField] private float upPanning = 5f;
    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        
    }
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.DownArrow)) //Aþaðý bakma
        {
            SetPosition(playerPos.x, playerPos.y - downPanning);
        
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            SetPosition(playerPos.x, playerPos.y + upPanning);
        }
        else
        {
            SetPosition(playerPos.x, playerPos.y); 
        }
    }
    private void SetPosition(float x, float y)
    {
        playerPos = playerTransform.position;
        Vector3 targetPosition = new Vector3(x, y, CAMERA_Z_POSITION) + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingValue * Time.fixedDeltaTime);
    }
}
