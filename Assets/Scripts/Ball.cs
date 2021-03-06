﻿using UnityEngine;

public class Ball : MonoBehaviour {

    public const float DEFAULT_BALL_HEIGHT = 1f;

    public delegate void RefreshEvent();
    event RefreshEvent refreshEvent;

    public BallCarrier owner;
    public BallCarrier Owner
    {
        get { return owner; }
        private set {
            if(owner != null)
            {
                GameLoop.carriersFree.Add(owner);
            }
            owner = value;
            if(value != null)
            {
                owner.ball = this; // make sure this isnt happening multiple times in code base
            }
            else
            {
                // if you were assigned NULL owner/carrier
                Debug.Log("Removed ball owner, adding to ball free list");
                GameLoop.ballsFree.Add(this);
            }
            if(refreshEvent != null) refreshEvent();
        }
    }
    
    private BallCarrier target;
    private Rigidbody rb;

    private bool isMoving;
    private float speed;
    
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (!isMoving)
        {
            if (Owner != null)
            {
                transform.position = Owner.transform.position + (Vector3.up * DEFAULT_BALL_HEIGHT); // set as a child instead?
            }
            return;
        }
        Vector3 dir = Vector3.Normalize(target.transform.position - transform.position); // rotate at all?
        rb.MovePosition(transform.position + (dir * speed * Time.deltaTime));
    }

    void ThrowTo(BallCarrier t)
    {
        if(isMoving)
        {
            Debug.Log("Can't throw the ball. Ball is already thrown.");
            return;
        }
        if (t.HasBall())
        {
            Debug.Log("Can't throw the ball. Target has a ball already.");
            return;
        }
        
        if(Owner != null)
        {
            transform.position = Owner.transform.position + (Vector3.up * DEFAULT_BALL_HEIGHT);
            speed = Owner.throwStrength;
        }
        else
        {
            // Should only happens on force gives (i.e. carrier didnt throw)
            speed = BallProperties.speed; 
        }

        target = t;
        Owner = target;
        isMoving = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(isMoving && target != null && other.GetComponent<BallCarrier>() == target)
        {
            //target.SendMessage("ReceiveBall", this);
            isMoving = false;
            // TODO Does does ball hide on possession? or do you visibly hold it somewhere? or just aura particles
        }
    }

    public void RegisterToRefresh(RefreshEvent e)
    {
        // Check to see if we actually should be doing a remove before add
        refreshEvent -= e;
        refreshEvent += e;
    }

    public void UnregisterToRefresh(RefreshEvent e)
    {
        refreshEvent -= e;
    }

    public void Capture(BallHunter bh)
    {
        if(Owner != null)
        {
            GameLoop.carriersFree.Add(Owner);
            Owner.ball = null;
        }
        //bh.Precious = null;
        Owner = null;
    }
}
