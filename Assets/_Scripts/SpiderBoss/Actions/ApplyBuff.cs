﻿using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class ApplyBuff : Action
{
	private BehaviorBlackboard _blackboard;

	public GameObject rightLegsStart;
	public GameObject rightLegsEnd;
	public GameObject leftLegsStart;
	public GameObject leftLegsEnd;

	private bool _intermediatePointReached;
	private Vector3 _targetPoint;
	private Vector3 _intermediatePoint;
	private Vector3 _startPoint;

	private Vector3 _shadowFinalPoint;
	private Vector3 _shadowIntermediatePoint;
	private Vector3 _shadowStartPoint;

	private float _lerpTime;

	private float _moveTime;
	private float _applicationTime;

	private bool _goingBack;
	private bool _finished;

	public enum BuffType
	{
		venom = 0,
		web = 1,
	}
	public BuffType _buffType;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		if(rightLegsStart == null ||
		   rightLegsStart == null ||
		   rightLegsStart == null ||
		   rightLegsStart == null)
		{
			Debug.Log("ERROR: no reference for buff application points on this leg");
		}
	}

	public override void OnStart ()
	{
		_blackboard.selectedLeg.GetComponent<LegScript>()._state = LegScript.LegState.CalculateMove;
		_blackboard.selectedLeg.GetComponent<LegScript>()._behaviorState = LegScript.BehaviorState.ApplyingBuff;
		_finished = false;
		_goingBack = false;
		_moveTime = 2.0f;
		_applicationTime = 2.0f;
	}

	//applies the buff to the selected leg
	public override TaskStatus OnUpdate()
	{
		if(_buffType != null)
		{
			//the animation to apply the buff! 
			if(_blackboard.selectedLeg.GetComponent<LegScript>()._state == LegScript.LegState.CalculateMove)
			{
				if(_blackboard.selectedLeg.GetComponent<LegScript>()._id < 4)	//these are the right legs
				{
					_targetPoint = rightLegsEnd.transform.position;
					_intermediatePoint = rightLegsStart.transform.position;
				}
				else if(_blackboard.selectedLeg.GetComponent<LegScript>()._id >= 4)	//these are the left legs
				{
					_targetPoint = leftLegsEnd.transform.position;
					_intermediatePoint = leftLegsStart.transform.position;
				}
				_startPoint = _blackboard.selectedLeg.GetComponent<LegScript>().transform.position;
				_shadowStartPoint = _startPoint;

				_shadowIntermediatePoint = _intermediatePoint;
				_shadowIntermediatePoint.y -= 3;
				_shadowIntermediatePoint.z = 0.9f;

				_shadowFinalPoint = _targetPoint;
				_shadowFinalPoint.y -= 2.5f;
				_shadowFinalPoint.z = 0.9f;
				
				_lerpTime = 0.0f;
				_intermediatePointReached = false;
				_blackboard.selectedLeg.GetComponent<LegScript>()._state = LegScript.LegState.Move;
				return TaskStatus.Running;
			}
			else if(_blackboard.selectedLeg.GetComponent<LegScript>()._state == LegScript.LegState.Move)
			{
				MoveLeg();
				if(_finished == true)
				{
					return TaskStatus.Success;
				}
				return TaskStatus.Running;
			}
		}
		//no buff type set? return fail
		return TaskStatus.Failure;
	}

	//function that moves the leg and sets the leg to idle when done
	void MoveLeg()
	{
		if(_intermediatePointReached == false)
		{
			if( Vector3.Distance(_blackboard.selectedLeg.transform.position, _intermediatePoint) < 0.01f)
			{
				_intermediatePointReached = true;
				_lerpTime = 0.0f;
			}
			else
			{
				_blackboard.selectedLeg.transform.position = Vector3.Lerp(_startPoint, _intermediatePoint, _lerpTime / _moveTime);
				_blackboard.selectedLeg.GetComponent<LegScript>()._shadowPos = Vector3.Lerp(_shadowStartPoint, _shadowIntermediatePoint, _lerpTime / _moveTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else
		{
			if( Vector3.Distance(_blackboard.selectedLeg.transform.position, _targetPoint) < 0.01f)
			{
				if(_goingBack == false)
				{
					BuffApplication();
				}
				else
				{
					AttackSystem.hitCircle((Vector2)_blackboard.selectedLeg.transform.position, 0.4f, 5.0f, -1);
					_finished = true;
				}
			}
			else
			{
				_blackboard.selectedLeg.transform.position = Vector3.Lerp(_intermediatePoint, _targetPoint, _lerpTime / _applicationTime);
				_blackboard.selectedLeg.GetComponent<LegScript>()._shadowPos = Vector3.Lerp(_shadowIntermediatePoint, _shadowFinalPoint, _lerpTime / _applicationTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
	}

	void BuffApplication()
	{
		if(_buffType == BuffType.venom)
		{
			//Debug.Log("applying venom");
			//have the selected leg move over to the mouth so that it can apply the buff and then return success
			_blackboard.selectedLeg.GetComponent<LegScript>().ApplyBuff(1);
			_goingBack = true;
		}
		else if(_buffType == BuffType.web)
		{
			//Debug.Log("applying web");
			_blackboard.selectedLeg.GetComponent<LegScript>().ApplyBuff(2);
			_goingBack = true;
		}
		CalculateBack();
	}

	void CalculateBack()
	{
		_intermediatePointReached = false;
		_lerpTime = 0.0f;
		_targetPoint = _startPoint;
		_startPoint = _blackboard.selectedLeg.GetComponent<LegScript>().transform.position;

		_intermediatePoint = _blackboard.points[_blackboard.selectedLeg.GetComponent<LegScript>()._id].transform.position;
		if(_blackboard.selectedLeg.GetComponent<LegScript>()._id != 0 &&
		   _blackboard.selectedLeg.GetComponent<LegScript>()._id != 4)
		{
			_intermediatePoint.y -= 2;
		}

		_moveTime = 1.0f;
		_applicationTime = 0.5f;

		_shadowIntermediatePoint = _intermediatePoint;
		_shadowIntermediatePoint.y -= 2.25f;

		_shadowStartPoint = _shadowFinalPoint;
		_shadowFinalPoint = _targetPoint;
	}
	
	public override void OnEnd ()
	{
		_blackboard.selectedLeg.GetComponent<LegScript>()._state = LegScript.LegState.Idle;
		_blackboard.selectedLeg.GetComponent<LegScript>()._behaviorState = LegScript.BehaviorState.Walking;
	}
}
