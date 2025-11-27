using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    Rigidbody2D playerRigidbody;
    float maxSpeed = 3.0f;
    float jumpPower = 100.0f;
    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        playerRigidbody.velocity = new Vector2(horizontalInput * maxSpeed, playerRigidbody.velocity.y);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerRigidbody.AddForce(Vector2.up * jumpPower);
        }
    }
}
