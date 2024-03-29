using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region State Variables
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerInAirState inAirState { get; private set; }
    public PlayerLandState landState { get; private set; }
    public PlayerCrouchIdleState crouchIdleState { get; private set; }
    public PlayerCrouchMoveState crouchMoveState { get; private set; }
    public PlayerSprintState sprintState { get; private set; }
    public PlayerClimbIdleState climbIdleState { get; private set; }
    public PlayerClimbMoveState climbMoveState { get; private set; }

    #endregion

    #region Components
    public Animator anim { get; private set; }
    public PlayerInputHandler inputHandler { get; private set; }
    public new Rigidbody2D rigidbody { get; private set; }

    [SerializeField]
    private PlayerData playerData;
    #endregion

    #region Check Transforms

    [SerializeField]
    public Transform groundCheck;
    public Transform ceilingCheck;

    #endregion

    #region Other Variables
    private Vector2 workspace;
    public Vector2 currentVelocity { get; private set; }
    public int facingDirection { get; private set; }
    public bool canFlip { get; private set; }
    #endregion

    #region Unity Callback Functions
    private void Awake()
    {
        stateMachine = new PlayerStateMachine();

        // TODO: animation not yet created
        idleState = new PlayerIdleState(this, stateMachine, playerData, "idle");
        moveState = new PlayerMoveState(this, stateMachine, playerData, "move");
        jumpState = new PlayerJumpState(this, stateMachine, playerData, "inAir");
        inAirState = new PlayerInAirState(this, stateMachine, playerData, "inAir");
        landState = new PlayerLandState(this, stateMachine, playerData, "land");
        crouchIdleState = new PlayerCrouchIdleState(this, stateMachine, playerData, "crouchIdle");
        crouchMoveState = new PlayerCrouchMoveState(this, stateMachine, playerData, "crouchMove");
        sprintState = new PlayerSprintState(this, stateMachine, playerData, "sprint");
        climbIdleState = new PlayerClimbIdleState(this, stateMachine, playerData, "climbIdle");
        climbMoveState = new PlayerClimbMoveState(this, stateMachine, playerData, "climbMove");
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = playerData.gravityScale;

        facingDirection = 1;

        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        currentVelocity = rigidbody.velocity;
        stateMachine.currentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.currentState.PhysicsUpdate();
    }
    #endregion

    #region Set Functions
    public void SetVelocityZero()
    {
        workspace.Set(0, 0);
        rigidbody.velocity = workspace;
        currentVelocity = workspace;
    }

    public void SetVelocityX(float velocity)
    {
        workspace.Set(velocity, currentVelocity.y);
        rigidbody.velocity = workspace;
        currentVelocity = workspace;
    }

    public void SetVelocityY(float velocity)
    {
        workspace.Set(currentVelocity.x, velocity);
        rigidbody.velocity = workspace;
        currentVelocity = workspace;
    }

    public void SetPosition(Vector2 position)
    {
        gameObject.transform.position = position;
    }

    public void SetCanFlip(bool canFlip)
    {
        this.canFlip = canFlip;
    }
    #endregion

    #region Check Functions
    public bool CheckIfGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            playerData.groundCheckRadius,
            playerData.ground
        );
    }

    public bool CheckIfHasCeiling()
    {
        return false;
    }

    public void CheckIfShouldFlip(int xInput)
    {
        if (xInput != 0 && xInput != facingDirection)
        {
            Flip();
        }
    }

    public bool CheckIfHasLadder()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            playerData.ladderCheckRadius,
            playerData.ladder
        ) || Physics2D.OverlapCircle(
            ceilingCheck.position,
            playerData.ladderCheckRadius,
            playerData.ladder);
    }

    #endregion

    #region Other Functions

    private void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();

    private void AnimationFinishedTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public float CalculateVelocityX(int xInput, float maxSpeed, float maxAcceleration)
    {
        Vector2 desiredVelocity = new Vector2(xInput, 0f) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        return (

            // Prevents player sliding when changing direction or stopping
            xInput == 0
            || (
                xInput != 0
                && currentVelocity.x != 0
                && Mathf.Sign(xInput) != Mathf.Sign(currentVelocity.x)
            )
        )
            ? Mathf.MoveTowards(currentVelocity.x, desiredVelocity.x, 5 * maxSpeedChange)
            : Mathf.MoveTowards(currentVelocity.x, desiredVelocity.x, maxSpeedChange);
    }

    public GameObject GetLadderObject()
    {
        if (CheckIfHasLadder())
        {
            GameObject bottomCheck = Physics2D.OverlapCircle(
                groundCheck.position,
                playerData.ladderCheckRadius,
                playerData.ladder
            ).gameObject;

            if (bottomCheck.CompareTag("Ladder")) {
                return bottomCheck;
            }

            GameObject topCheck = Physics2D.OverlapCircle(
                ceilingCheck.position,
                playerData.ladderCheckRadius,
                playerData.ladder
            ).gameObject;

            if (topCheck.CompareTag("Ladder")) {
                return topCheck;
            }
        }

        return null;
    }

    #endregion
}
