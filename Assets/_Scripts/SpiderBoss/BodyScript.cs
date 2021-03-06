﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyScript : MonoBehaviour 
{
	[HideInInspector]
	public float _groundHeightOffset;

	[HideInInspector]
	public GameObject _neutralPoint;

	public float _baseHeight;
	public float _height;

	[HideInInspector]
	public enum BehaviorState
	{
		Dead = -3,
		Dying = -2,
		Disabled = -1,
		Healthy = 0,
		BodySlam = 1,
		MegaFlare = 2,
		Suction = 3,
		Lob = 4,
		CrouchingCharge = 5,
	}
	public BehaviorState _behaviorState;

	[HideInInspector]
	public enum BodyState
	{
		Floating = 0,
		Falling = 1,
		OnGound = 2,
		Rising = 3,
		Charging = 4,
		Recovery = 5,
		Attacking = 6,
	}
	public BodyState _bodyState;

	[HideInInspector]
	public Vector2 _shadowPos;

	// Use this for initialization
	void Start () 
	{
		_baseHeight = 3.5f;
		_groundHeightOffset = 0.35f;
		_height = _baseHeight;
		_behaviorState = BehaviorState.Healthy;
		_bodyState = BodyState.Floating;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_behaviorState == BehaviorState.Healthy)
		{
			_shadowPos = (Vector2)transform.position;
			_shadowPos.y -= _height;

		}

	}
}
