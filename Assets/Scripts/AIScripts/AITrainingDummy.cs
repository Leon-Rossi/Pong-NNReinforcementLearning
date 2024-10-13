using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITrainingDummy : Racket
{
    public Transform ball;
    protected override void Movement()
    {
        GetComponent<Rigidbody2D>().transform.position = new Vector3(GetComponent<Rigidbody2D>().transform.position.x,ball.position.y, 0);
    }

}
