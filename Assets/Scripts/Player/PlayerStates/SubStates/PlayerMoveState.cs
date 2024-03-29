using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(
        Player player,
        PlayerStateMachine stateMachine,
        PlayerData playerData,
        string animBoolName
    )
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.CheckIfShouldFlip(xInput);
        player.SetVelocityX(
            player.CalculateVelocityX(
                xInput,
                playerData.moveMaxSpeed,
                playerData.moveMaxAcceleration
            )
        );

        if (!isExitingState)
        {
            if (xInput == 0)
            {
                stateMachine.ChangeState(player.idleState);
            }
            else if (crouchInput)
            {
                stateMachine.ChangeState(player.crouchMoveState);
            }
            else if (sprintInput)
            {
                stateMachine.ChangeState(player.sprintState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }
}
