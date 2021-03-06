﻿using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class PauseResumeBehavior : Action
{
	[HideInInspector]
	public enum Pause
	{
		PauseBehavior = 0,
		ResumeBehavior = 1,
		StopBehavior = 2,
	}
	public Pause _pause;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override TaskStatus OnUpdate()
	{
		if(_blackboard._currentBehavior != null)
		{
			if(_pause == Pause.PauseBehavior)
			{
				_blackboard._currentBehavior.Action.DisableBehavior();
			}
			else if(_pause == Pause.ResumeBehavior)
			{
				//disabling this so that we don't restart the same behavior but pick a new one.
				//_blackboard._currentBehavior.Action.enableBehavior();
				_blackboard.attackPatternStopped = false;
			}
			else if(_pause == Pause.StopBehavior)
			{
				_blackboard.attackPatternStopped = true;
				_blackboard._currentBehavior.Action.DisableBehavior();
			}
			return TaskStatus.Success;
		}
		else
		{
			Debug.Log("WARNING: no behavior to pause");
			return TaskStatus.Success;
		}
	}
}
