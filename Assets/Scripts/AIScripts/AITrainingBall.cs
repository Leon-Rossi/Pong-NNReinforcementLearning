using System;
using UnityEngine;

public class AITrainingBall : MonoBehaviour
{
    public Racket LeftRacket, RightRacket;
    public Rigidbody2D ballRb;
    public float moveSpeed;

    float lastXPosition;

    void Start()
    {
        ballRb.velocity = new Vector2(1, 1) * moveSpeed;
    }

    public void Restart()
    {
        ballRb.gameObject.transform.position = new Vector3(0,0,0);
        ballRb.velocity = new Vector2(-1, 1) * moveSpeed;
    }
        
    private void FixedUpdate()
    {
        if(gameObject.transform.position.y > 5 || gameObject.transform.position.y < -5)
        {
            Restart();
        }

        if(Math.Abs(ballRb.transform.position.x - lastXPosition) < 0.001)
        {
            Restart();
        }

        lastXPosition = ballRb.transform.position.x;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TagMenager tagMenager = collision.gameObject.GetComponent<TagMenager>();

        GetComponent<AudioSource>().Play();

        if (tagMenager == null)
        {
            return;
        }

        Tag tag = tagMenager.wallTag;

        if (tag.Equals(Tag.leftWall))
        {
            RightRacket.GetScore();
        }

        if (tag.Equals(Tag.rightWall))
        {
            LeftRacket.GetScore();
            RightRacket.OnHit();
        }
        if (tag.Equals(Tag.leftRacket))
        {
            wayBallLeft(collision, 1);
        }
        if (tag.Equals(Tag.rightRacket))
        {
            wayBallRight(collision, -1);
            RightRacket.OnHit();
        }
    }

    private void wayBallRight(Collision2D collision, int x)
    {
        float a = transform.position.y - collision.gameObject.transform.position.y;
        float b = collision.collider.bounds.size.y;
        float y = a / b;
        ballRb.velocity = new Vector2(x, y)*moveSpeed;
    }

    private void wayBallLeft(Collision2D collision, int x)
    {
        float y = UnityEngine.Random.value;
        if(UnityEngine.Random.value > 0.5)
        {
            y *= -1;
        }

        ballRb.velocity = new Vector2(x, y)*moveSpeed;
    }
}
