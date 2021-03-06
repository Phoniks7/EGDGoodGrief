﻿using UnityEngine;
using System.Collections;

public class TetherProjectileScript : MonoBehaviour 
{
	public float _moundHP;
	public float _moundHPMax = 100.0f; 

	public GameObject _tetherShot;
	public GameObject _webTether;
	public GameObject _targetPlayer;
	public GameObject _tetherHook;

	private Vector2 _startPos;
	private Vector2 _playerPos;
	private Vector2 _endPos;
	private Vector2 _directionVector;
	//private float _rotationAngle;

	public float _tetherDistance = 1.0f;
	public float _flightDuration = 0.3f;
	public float _settingDuration = 0.1f;
	public float _lerpTime = 0.0f;

	private ScheduledColor scheduledColor = new ScheduledColor( new Color( 1.0f, 1.0f, 1.0f, 1.0f), 0.0f);
	private float colorT = 0.0f;

	public enum BehaviorState
	{
		Moving = 0,
		Setting = 1,
		Set = 2,
	}
	public BehaviorState _state;

	// Use this for initialization
	void Start () 
	{
		_moundHP = _moundHPMax;
		_state = BehaviorState.Moving;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(_state == BehaviorState.Moving)
		{
			if(Vector3.Distance(_playerPos, transform.position) < 0.001f)
			{
				//GameObject tether = Instantiate(_webTether, transform.position, Quaternion.identity) as GameObject;
				//tether.GetComponent<TetherScript>().Initializer(_targetPlayer, _directionVector);
				_tetherShot.SetActive(false);
				_webTether.SetActive(true);
				_tetherHook.GetComponent<TetherHook>()._parent = _targetPlayer;
				_endPos = _playerPos + _directionVector * _tetherDistance;
				_lerpTime = 0.0f;

				_targetPlayer.GetComponent<Player>().KnockBack(0.0f, Vector2.zero);
				_state = BehaviorState.Setting;
			}
			else
			{
				_playerPos = _targetPlayer.transform.position;
				float xDiff = _playerPos.x - transform.position.x; 
				float yDiff = _playerPos.y - transform.position.y; 
				float rotationAngle = Mathf.Atan2(yDiff, xDiff) * (180.0f / Mathf.PI);
				transform.eulerAngles = new Vector3(0, 0, rotationAngle);

				transform.position = Vector2.Lerp(_startPos, _playerPos, _lerpTime / _flightDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == BehaviorState.Setting)
		{
			if(Vector3.Distance(_endPos, transform.position) < 0.001f)
			{
				_state = BehaviorState.Set;
			}
			else
			{
				transform.position = Vector2.Lerp(_playerPos, _endPos, _lerpTime / _settingDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == BehaviorState.Set)
		{
			if(_moundHP <= 0)
			{
				Destroy(this.gameObject);
			}
			AttackSystem.Tether(_targetPlayer, transform.position, 0.5f, 3.0f, Time.deltaTime * StaticData.t_scale );
		}

		//Color handling logic.
		colorT = Mathf.Max ( 0.0f, colorT - Time.deltaTime * StaticData.t_scale );
		if ( colorT == 0.0f && scheduledColor.duration != 0.0f )
		{
			scheduledColor.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
			scheduledColor.duration = 0.0f;
			SpriteRenderer[] temp = GetComponentsInChildren<SpriteRenderer>();
			foreach ( SpriteRenderer s in temp )
			{
				s.color = scheduledColor.color;
			}
		}
	}
	
	public void Initializer(Vector2 startPos, GameObject targetPlayer)
	{
		_targetPlayer = targetPlayer;
		transform.position = startPos;
		_startPos = transform.position;
		_playerPos = targetPlayer.transform.position;
		float xDiff = _playerPos.x - startPos.x; 
		float yDiff = _playerPos.y - startPos.y; 
		float rotationAngle = Mathf.Atan2(yDiff, xDiff) * (180.0f / Mathf.PI);
		
		transform.eulerAngles = new Vector3(0, 0, rotationAngle);

		//calcualte the end point of the tether
		_directionVector = new Vector2(xDiff, yDiff);
		_directionVector.Normalize();
	}

	public void Hurt( float damage, int id )
	{
		if ( id == _targetPlayer.GetComponent<Player>().id ) //TODO: assign this script the id of the player it's tethering, replace 0 with that var.
		{
			//tethered player attacking, take ~ no damage.
			_moundHP -= damage * 0.1f;
		}
		else
		{
			//a player assists the tethered player.
			_moundHP -= _moundHPMax;
			GameState.cooperationAxis = Mathf.Min ( 1.0f, GameState.cooperationAxis + 0.05f );
		}

		//Color handling logic.
		scheduledColor.color = new Color( 1.0f, 0.5f, 0.5f, 1.0f );
		scheduledColor.duration = 0.15f;
		colorT = scheduledColor.duration;
		SpriteRenderer[] temp = GetComponentsInChildren<SpriteRenderer>();
		foreach ( SpriteRenderer s in temp )
		{
			s.color = scheduledColor.color;
		}

		if ( _moundHP <= 0 )
		{
			Destroy(this.gameObject);
		}
	}
}
