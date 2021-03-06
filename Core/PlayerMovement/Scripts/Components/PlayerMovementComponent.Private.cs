﻿
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using OpenMMO;

namespace OpenMMO {
	
	// ===================================================================================
	// PlayerMovement
	// ===================================================================================
	public partial class PlayerMovementComponent
	{
		
		[Header("Performance")]
		[Tooltip("How often are movement updates sent to the server (in seconds)?")]
		[Range(0.01f, 99)]
		public double movementUpdateInterval = 1f;
		
		double _timerMovement = 0;
		
		// -------------------------------------------------------------------------------
		// CheckMovementInterval
		// -------------------------------------------------------------------------------
		protected bool CheckMovementInterval => Time.time > _timerMovement;
		
		// -------------------------------------------------------------------------------
		// RefreshMovementInterval
		// -------------------------------------------------------------------------------
		void RefreshMovementInterval()
		{
			_timerMovement = Time.time + movementUpdateInterval;
		}
		
		// -------------------------------------------------------------------------------
		// FixedUpdateClient
		// -------------------------------------------------------------------------------
		[Client]
		protected override void FixedUpdateClient()
    	{
        	if (isLocalPlayer									// only for local Player
        		&& CheckMovementInterval						// we throttle a little bit
        		)
        	{
				Cmd_UpdateState(new MovementStruct(verticalMovementInput, horizontalMovementInput, running));
				RefreshMovementInterval();
			}
        	
		}
		
		// -------------------------------------------------------------------------------
		// Cmd_UpdateState
		// This updates the movement input Axis on the server and then does calculations
		// based on the new values
		// @Client -> @Server
		// -------------------------------------------------------------------------------
		[Command]
		protected virtual void Cmd_UpdateState(MovementStruct movementStruct)
    	{
    		
    		verticalMovementInput 		= Mathf.Clamp(movementStruct.verticalMovementInput, -1, 1);		// good enough for keyboard + controller
    		horizontalMovementInput 	= Mathf.Clamp(movementStruct.horizontalMovementInput, -1, 1);	// good enough for keyboard + controller
    		running						= movementStruct.movementRunning;
    		
    		UpdateVelocity();
		}
		
		// -------------------------------------------------------------------------------
		// UpdateVelocity
		// This recalculates the agent velocity based on the current input axis'
		// -------------------------------------------------------------------------------
		protected virtual void UpdateVelocity()
		{
			
			if (verticalMovementInput != 0 || horizontalMovementInput != 0)
           	{
            	
            	agent.updateRotation = false;
            	
				Vector3 input = new Vector3(horizontalMovementInput, 0, verticalMovementInput);
				if (input.magnitude > 1) input = input.normalized;

				Vector3 angles = transform.rotation.eulerAngles;
				angles.x = 0;
				Quaternion rotation = Quaternion.Euler(angles);

				Vector3 direction = rotation * input;
				
				if (verticalMovementInput > 0)									// -- Movement: Forward
				{
					float factor = running ? runFactor : walkFactor;
					agent.velocity = direction * verticalMovementInput * agent.speed * factor;
				}
				else if (verticalMovementInput < 0)								// -- Movement: Backward
				{
					agent.velocity = direction * Mathf.Abs(verticalMovementInput) * agent.speed * backwardFactor;
				}
				else
					agent.velocity = Vector3.zero; // -- Is this really required?
				
           	}
           	else
           	{
           		// -- Is this really required?
         	  	agent.ResetPath();
				agent.velocity = Vector3.zero;
			}
			
		}
		
		// -------------------------------------------------------------------------------
		
	}

}

// =======================================================================================