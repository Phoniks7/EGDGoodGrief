﻿using UnityEngine;
using System.Collections;

public class LegScript : MonoBehaviour 
{
	#region vars
	public int _id;

	public float _radius = 2.5f;
	public float _movementFlux = 0.75f;
	public float _moveTime;
	public GameObject _disabledPoint;
	public GameObject _radiusPoint;
	public GameObject _bodyScript;
	public GameObject _bloodAnchor;

	private bool _recovering;
	public float _currentHP;
	public float _maxHP;
	public float _regenRate;
	public bool _invincible; //true when the leg is immune to damage.

	//color vars
	ScheduledColor currentColor;

	//sound vars
	private float soundDelay = 0.5f; //seconds between hurt sound being played? TODO: better solution.
	private float soundTimer = 0.0f;

	#region LEG BUFF VARIABLES
	public GameObject _webBuff;
	public GameObject _venomBuff;

	public float _currentWebbingHP;
	public float _maxWebbingHP;

	public float _currentBuffDuration;
	public float _maxBuffDuration;

	public float _poisonDPS;      //in damage per second
	public float _poisonDuration; //in seconds
	#endregion

	[HideInInspector]
	public Vector2 _shadowPos;
	[HideInInspector]
	public Vector2 _shadowIntermediatePoint;
	[HideInInspector]
	public float _shadowMoveTime;
	private Vector2 _shadowTargetPoint;
	private Vector2 _shadowStartPoint;

	private bool _intermediatePointReached;
	private Vector3 _intermediatePoint;
	private Vector3 _targetPoint;
	[HideInInspector]
	public Vector2 _startPoint;
	[HideInInspector]
	public float _lerpTime;
	#endregion

	public BehaviorBlackboard _blackboard;
	public GameObject bloodPrefab;

	//enums
	public enum BehaviorState
	{
		Dead = -2,
		Disabled = -1,
		Walking = 0,
		Impale = 1,
		Rake = 2,
		ApplyingBuff = 3,
		MegaFlareUnused = 4,
	}
	public BehaviorState _behaviorState;

	public enum LegState
	{
		Idle = 0,
		CalculateMove = 1,
		Move = 2,
	}
	[HideInInspector]
	public LegState _state;

	public enum BuffState
	{
		unbuffed = 0,
		venom= 1,
		web = 2,
	}
	//[HideInInspector]
	public BuffState _buffState;

	// Use this for initialization
	void Start () 
	{
		_buffState = BuffState.unbuffed;
		_behaviorState = BehaviorState.Walking;
		_state = LegState.Idle;

		_regenRate = 10.0f;
		_moveTime = 0.25f;
		_maxHP = 1400.0f;
		_currentHP = _maxHP;

		_maxWebbingHP = 1000.0f;
		_currentWebbingHP = 0.0f;

		_maxBuffDuration = 45.0f;
		_currentBuffDuration = 0.0f;

		currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 1.0f), 0.0f );
		_recovering = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_behaviorState == BehaviorState.Disabled)
		{
			if(_state == LegState.Idle)
			{
				transform.position = _disabledPoint.transform.position;

				_shadowPos = (Vector2)transform.position;
				if(_blackboard.body._behaviorState != BodyScript.BehaviorState.Disabled)
				{
					//_shadowPos.y -= 3.0f;
					_shadowPos.y -= _blackboard.body._height;
				}
			}
			else if(_state == LegState.CalculateMove)
			{
				transform.parent.rigidbody2D.fixedAngle = true;
				Vector3 rotation = new Vector3(0, 0, 0.0f);
				if(_id == 7)
					rotation.z = 19.0f;
				else if(_id == 6)
					rotation.z = 26.0f;
				else if(_id == 5)
					rotation.z = 37.0f;
				else if(_id == 4)
					rotation.z = 58.0f;
				else if(_id == 3)
					rotation.z = -19.0f;
				else if(_id == 2)
					rotation.z = -26.0f;
				else if(_id == 1)
					rotation.z = -37.0f;
				else if(_id == 0)
					rotation.z = -58.0f;
				transform.parent.transform.eulerAngles = rotation;

				_targetPoint = _disabledPoint.transform.position;
				_shadowTargetPoint = _targetPoint;
				_shadowTargetPoint.y -= _blackboard.body._height;

				_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, 0);
				_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
				_intermediatePointReached = false;
				_startPoint = transform.position;
				_lerpTime = 0.0f;
				_state = LegState.Move;

			}
			else if(_state == LegState.Move)
			{
				MoveLeg();
			}
		}
		else if(_behaviorState == BehaviorState.Walking)
		{
			if(_state == LegState.Idle)
			{
				//check to see if the leg is too far away from the radius point 
				if(Vector2.Distance(this.transform.position, _radiusPoint.transform.position) > _radius)
				{
					_state = LegState.CalculateMove;
				}
				_invincible = false;
				_shadowPos = (Vector2)transform.position;
			}
			//This picks the next area to move to
			else if(_state == LegState.CalculateMove)
			{
				_targetPoint = _radiusPoint.transform.position + GetMoveVector();
				_shadowTargetPoint = _targetPoint;
				if(_recovering)
				{
					_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, -1);
					_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
					_recovering = false;
				}
				else
				{
					_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, 1);
					_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
				}
				_intermediatePointReached = false;
				_startPoint = transform.position;
				_shadowStartPoint = _startPoint;
				_lerpTime = 0.0f;
				_invincible = true;
				_state = LegState.Move;
			}
			else if(_state == LegState.Move)
			{
				MoveLeg();
			}
		}
		else if(_behaviorState == BehaviorState.Impale)
		{
			_shadowPos = Vector2.Lerp(_startPoint, _shadowIntermediatePoint, _lerpTime / _shadowMoveTime);
			_lerpTime += (Time.deltaTime* StaticData.t_scale);
		}
		else if(_behaviorState == BehaviorState.ApplyingBuff)
		{
			_invincible = true;
		}

		//get the rotation
		transform.rotation = transform.parent.localRotation;

		HandleStats();
	}

	Vector3 GetMoveVector()
	{
		return (Vector3)_bodyScript.GetComponent<BehaviorBlackboard>().moveDirection * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
		//return _bodyScript.GetComponent<BodyScript>().move_vec * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
	}

	//returns the midpoint of ab and sets it up a bit
	Vector3 GetIntermediatePoint(Vector3 a, Vector3 b, int upOffset)
	{
		float xPoint = a.x + ((b.x - a.x)/2);
		float yPoint = a.y + ((b.y - a.y)/2);
		float zPoint = a.z + ((b.z - a.z)/2);
		//_shadowIntermediatePoint = new Vector3(xPoint, yPoint, zPoint);

		if(upOffset == 1)
		{
			yPoint += _radius * 1.6f;
		}
		else if(upOffset == -1)
		{
			yPoint -= _radius;
		}

		if(upOffset != 0)
		{
			if(_blackboard.moveDirection.y > 0.5f)
			{
				yPoint = b.y + _radius;
			}
			else if(_blackboard.moveDirection.y < -0.5f)
			{
				yPoint = a.y + _radius;
			}
		}
		return new Vector3(xPoint, yPoint, zPoint);
	}

	//function that moves the leg and sets the leg to idle when done
	void MoveLeg()
	{
		if(_intermediatePointReached == false)
		{
			if( Vector2.Distance(transform.position, _intermediatePoint) < 0.01f)
			{
				_intermediatePointReached = true;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector3.Lerp(_startPoint, _intermediatePoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_shadowStartPoint, _shadowIntermediatePoint, _lerpTime / _moveTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else
		{
			if( Vector2.Distance((Vector2)transform.position, _targetPoint) < 0.01f)
			{
				AttackSystem.hitCircle((Vector2)transform.position, 0.4f, 5.0f, -1);
				GameState.cameraController.Shake (0.01f, 0.1f );
				_state = LegState.Idle;

				if(_behaviorState == BehaviorState.Walking)
				{
					transform.parent.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossStep, 0.75f );
				}
			}
			else
			{
				transform.position = Vector3.Lerp(_intermediatePoint, _targetPoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_shadowIntermediatePoint, _shadowTargetPoint, _lerpTime / _moveTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
	}

	//this handles the logic for the stats on the boss like health and buffs
	void HandleStats()
	{
		if ( currentColor.duration > 0.0f )
		{
			if ( currentColor.timer >= currentColor.duration )
			{
				currentColor.duration = 0.0f;
				if(_buffState == BuffState.web)
				{
					_webBuff.GetComponent<SpriteRenderer>().color = getResetColor();
				}
				else
				{
					transform.parent.GetComponent<SpriteRenderer>().color = getResetColor ();
				}
			}
			else
			{
				if(_buffState == BuffState.web)
				{
					_webBuff.GetComponent<SpriteRenderer>().color = currentColor.color;
				}
				else
				{
					transform.parent.GetComponent<SpriteRenderer>().color = currentColor.color;
				}
			}
			currentColor.timer += Time.deltaTime * StaticData.t_scale;
		}

		if (_behaviorState != BehaviorState.Disabled)
		{
			//is the leg dead?
			if(_currentHP <= 0.0f)
			{
				_behaviorState = BehaviorState.Disabled;
				_state = LegState.CalculateMove;
				_shadowStartPoint = transform.position;
				_invincible = true;

				Vector3 Legscale = transform.parent.transform.localScale;
				Legscale.x = 2.0f;
				Legscale.y = 1.25f;
				transform.parent.transform.localScale = Legscale;

				//set the cooperation Valance
				GameState.cooperationAxis += Mathf.Min(0.1f , 1.0f - GameState.cooperationAxis);
			}
			//if not, do other things
			else
			{
				if(_buffState != BuffState.unbuffed)
				{
					if(_currentBuffDuration > 0.0f)
					{
						_currentBuffDuration -= Time.deltaTime * StaticData.t_scale;
					}
					else
					{
						RemoveBuff();
					}
				}
			}
		}
		else if(_behaviorState == BehaviorState.Disabled)
		{
			if(_currentHP == _maxHP)
			{
				transform.parent.rigidbody2D.fixedAngle = false;

				_recovering = true;

				Vector3 Legscale = transform.parent.transform.localScale;
				Legscale.x = 2.25f;
				Legscale.y = 2.25f;
				transform.parent.transform.localScale = Legscale;
				_behaviorState = BehaviorState.Walking;
			}
			/*
			else if(_currentHP < _maxHP)
			{
				if(_blackboard.body._behaviorState != BodyScript.BehaviorState.Disabled)
				{
				
				}
			}
			*/
		}
	}

	public void StartBuff(int buffID, float transitTime)
	{
		BuffState _state = (BuffState)buffID;
		if(_state == BuffState.venom)
		{
			if(_buffState == BuffState.web)
			{
				_webBuff.GetComponent<WebBuffScript>().Unapply();
			}
			_venomBuff.GetComponent<VenomBuffScript>().StartApplication(transitTime);
		}
		else if(_state == BuffState.web)
		{
			if(_buffState == BuffState.venom)
			{
				_venomBuff.GetComponent<VenomBuffScript>().Unapply();
			}
			_webBuff.GetComponent<WebBuffScript>().StartApplication(transitTime);
		}
	}

	public void ApplyBuff(int buffID)
	{
		_buffState = (BuffState)buffID;
		if(_buffState == BuffState.venom)
		{
			//transform.parent.GetComponent<SpriteRenderer>().color = Color.green;
			_poisonDPS = 2.5f;
			_poisonDuration = 5.0f;
			_currentBuffDuration = _maxBuffDuration;
		}
		else if(_buffState == BuffState.web)
		{
			//transform.parent.GetComponent<SpriteRenderer>().color = Color.blue;
			_currentWebbingHP = _maxWebbingHP;
			_currentBuffDuration = _maxBuffDuration;
		}
	}
	void RemoveBuff()
	{
		if(_buffState == BuffState.venom)
		{
			_venomBuff.GetComponent<VenomBuffScript>().Unapply();
		}
		else if(_buffState == BuffState.web)
		{
			_webBuff.GetComponent<WebBuffScript>().Unapply();
		}

		_buffState = BuffState.unbuffed;
		//reset vars (bookkeeping)
		_currentWebbingHP = 0.0f;
		_poisonDPS = 0.0f;
		_poisonDuration = 0.0f;
		_currentBuffDuration = 0.0f;
		transform.parent.GetComponent<SpriteRenderer>().color = Color.white;
	}

	public bool isPoisonous()
	{
		//returns whether or not the leg deals extra poison damage.
		return (_currentBuffDuration > 0.0f && _poisonDPS > 0.0f);
	}

	public Color getResetColor()
	{
		//returns the color the leg should be, given it's current state
		if ( _currentWebbingHP > 0.0f )
		{
			return new Color( 1.0f, 1.0f, 1.0f );
			//return new Color( 0.0f, 0.0f, 1.0f );
		}
		else if ( isPoisonous () )
		{
			return new Color( 1.0f, 1.0f, 1.0f );
			//return new Color( 0.0f, 1.0f, 0.0f );
		}
		else
		{
			return new Color( 1.0f, 1.0f, 1.0f );
		}
	}

	public void Hurt( float damage, int id )
	{
		//Handle players doing damage to the leg.
		if ( _invincible ) { return; } //immune to damage
		if ( _currentHP <= 0.0f ) { return; } //already dead
		//deal damage.

		//flash red.
		currentColor.color = new Color( 1.0f, 0.5f, 0.5f );
		currentColor.duration = 0.10f;
		currentColor.timer = 0.0f;

		//Spray?
		GameObject obj = (GameObject)Instantiate ( bloodPrefab, _bloodAnchor.transform.position, Quaternion.identity );
		float x = GameState.players[id].transform.position.x - transform.position.x;
		float y = GameState.players[id].transform.position.y - transform.position.y;
		float angle = Mathf.Atan2 ( y, x ) * Mathf.Rad2Deg - 90.0f;
		obj.transform.Rotate ( 0.0f, 0.0f, angle );
		obj.transform.parent = transform;

		GetComponent<DamageNumbersForBoss>().AddTakeDamagePoints ( id, damage );

		//Play hurt sound?
		if ( soundTimer <= 0.0f )
		{
			if ( _currentHP <= 0.0f )
			{
				//play dead sound
				transform.parent.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossLegBreak, 1.0f );
			}
			else
			{
				//play random hurt sound
				transform.parent.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.KnightSlice, 1.0f );
				/*
				float rng = Random.Range ( 0.0f, 100.0f );
				float possibilities = 2.0f;
				if ( rng <= 1.0f * 100.0f / possibilities )
				{
					transform.parent.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossHit1, 1.0f );
				}
				else //if ( rng <= 2.0f * 100.0f / possibilities )
				{
					transform.parent.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossHit2, 1.0f );
				}*/
			}
			soundTimer = soundDelay;
		}
		soundTimer = Mathf.Max ( 0.0f, soundTimer - Time.deltaTime * StaticData.t_scale );

		if ( _currentWebbingHP > 0.0f )
		{
			//Redirect 90% to the armor
			float redirectionPercent = 0.90f;
			_currentWebbingHP -= damage * redirectionPercent;
			_currentHP -= damage * (1.0f - redirectionPercent);
			ScoreManager.DealtDamage( id, damage );
			//ScoreManager.DealtDamage ( id, damage * (1.0f - redirectionPercent) ); //if armor damage doesn't give score
			//Edge case check: if armor was broken, deal the excess damage
			if ( _currentWebbingHP < 0.0f )
			{
				RemoveBuff();
				_currentHP += _currentWebbingHP;
				_currentWebbingHP = 0.0f;
				//ScoreManager.DealtDamage ( id, _currentWebbingHP ); //if armor damage doesn't give score
				ScoreManager.BrokeArmor( id );
			}
		}
		else
		{
			//Normal damage
			_currentHP -= damage;
		}
		#region player score
		if ( id >= 0 && id < 4 )
		{
			//Give the player score ~ damage
			ScoreManager.DealtDamage( id, damage );
			//Give the player bonus score for killing blow
			if ( _currentHP <= 0.0f )
			{
				RemoveBuff();
				ScoreManager.KilledLeg( id );
			}

			//increase player threat for dealing damage
			GameState.playerThreats[id] += 0.25f;
		}
		#endregion

		//sets the anger valence
		GameState.angerAxis += Mathf.Min(0.0025f , 1.0f - GameState.angerAxis);
		GameState.cooperationAxis += Mathf.Min(0.001f , 1.0f - GameState.cooperationAxis);

		//callback player
		GameState.playerStates[id].OnHitCallback();
	}
}
