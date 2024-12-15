using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITrainingDummy : Racket
{
    public Transform ball;
    protected override void Movement()
    {
        if (ball.position.y > transform.position.y)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * moveSpeed;
        }

        if (ball.position.y < transform.position.y)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1) * moveSpeed;
        }
    }

}
