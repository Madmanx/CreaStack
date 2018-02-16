using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongBehavior : MonoBehaviour {

    // The delay before the movement starts
    public float delayBeforeMovementStarts = 0.0f;

    // The delay between a ping pong mouvement
    public float delayTimer;

    private Vector3 lerpOriginPosition;
    private Vector3 lerpNewPosition;
    private bool isInPong;
    private float moveLerpValue = 0.0f;

    private float movingSpeed;

    // Use this for initialization
    void Start () {
        isInPong = true;

        movingSpeed = GameManager.Instance.movingSpeed;
        lerpOriginPosition = transform.position;
        lerpNewPosition = lerpOriginPosition + 2 * GameManager.Instance.currentOffset;
    }
	
	// Update is called once per frame
	void Update () {
        HandleMovingProcess();
    }

    public void HandleMovingProcess()
    {
        if (delayBeforeMovementStarts < 0.0f)
        {
            if (delayTimer < 0.0f)
            {
                transform.position = Vector3.Lerp(lerpOriginPosition, lerpNewPosition, moveLerpValue);
                moveLerpValue += Time.deltaTime * movingSpeed * ((!isInPong) ? 1 : -1);
                if ((Vector3.Distance(transform.position, lerpNewPosition) < 0.1f && !isInPong)
                || (Vector3.Distance(transform.position, lerpOriginPosition) < 0.1f && isInPong))
                {
                    isInPong = !isInPong;
                    moveLerpValue = (isInPong) ? 1.0f : 0.0f;
                }
            }
            else
            {
                delayTimer -= Time.deltaTime;
            }
        }
        else
        {
            delayBeforeMovementStarts -= Time.deltaTime;
        }
    }
}
