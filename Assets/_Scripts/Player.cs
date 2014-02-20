﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{

	#region vars
	float HP;
	float maxHP;
	float baseMaxHP;

	float defense = 1.0f; //defensive power: (2x = 1/2 damage). Base: 1 = 1x damage.
	float offense = 1.0f; //offensive power: (2x = 2x  damage). Base: 1 = 1  damage.

	string characterclass; //enum this

	//color / player id?

	//state data?
	public bool canMove = true;           //disable moving while attacking?
	public float speedMultiplier = 1.0f;  //able to move, but at a slower pace?
	//bool overrideMoveAni;

	public bool isDodging = false;   //currently dodging?
	public float dodgeTimer = 0.0f;  //time left in dodge animation?

	public bool isParrying = false;  //currently parrying?
	public float parryTimer = 0.0f;  //time left in parry animation?

	//Downed / Carrying State Stuff
	public bool isDowned = false;    //if you're downed
	public bool isCarrier = false;   //if you're carrying another player.
	public bool isCarried = false;   //if you're being carried by another player.
	public GameObject Carried;       //ref to the player you're carrying.
	public GameObject Carrier;       //ref to the player carrying you.
	public Vector2 carryVec;         //unit vector representing your carry direction.

	//interruption
	private float interruptHP = 0.0f;  //"interrupt hp": if this reaches 0, you get interrupted. Set by moves.
	private float interruptDR = 0.0f;  //interrupt diminishing returns factor. 
	                                   //  (makes you harder to interrupt the more you get interrupted)

	//Buff / Debuff data?
	public bool isInBulletTime = false;     //Immune to the stop watch slow?
	float bulletTimeDuration = 0.0f;        //How long (real time) until time control immunity expires.

	bool isPoisoned = false;
	float poisonDPS = 1.0f;
	float poisonDuration = 0.0f;

	//Speed buff
	//Regen buff
	//Defense buff
	//Attack buff

	//items x 3
	//item cooldowns?
	public Item[] items = new Item[3];
	int itemIndex = 0;
	const int ITEM_SLOT_COUNT = 3;

	//unique mechanic data
	float style;
	float focus;
	float chain;
	float accumulation;

	#endregion

	// Use this for initialization
	void Start () 
	{
		//initialize stats

		maxHP = baseMaxHP;
		HP = maxHP;

		offense = 1.0f;
		defense = 1.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float t = Time.deltaTime;
		if ( ! isInBulletTime ) { t = t * StaticData.t_scale; }

		//Timer countdown
		#region timers
		if ( isDodging ) 
		{ 
			dodgeTimer -= t;
			if ( dodgeTimer <= 0.0f )
			{
				isDodging = false;
			}
		}

		if ( isParrying )
		{
			parryTimer -= t;
			if ( parryTimer <= 0.0f )
			{
				isParrying = false;
			}
		}

		if ( isInBulletTime )
		{
			bulletTimeDuration -= t;
			if ( bulletTimeDuration <= 0 )
			{
				isInBulletTime = false;
			}
		}
		#endregion
	}

	void Respawn()
	{
		//state reset?
		maxHP = baseMaxHP;
		HP = maxHP;
	}

	public void ChangeItemIndex( int increment )
	{
		//increment: +1 or -1
		itemIndex = Mod( itemIndex + increment, ITEM_SLOT_COUNT );
	}

	int Mod(int x, int m) 
	{
		//Custom modulus operator
		//To fix the stupid % returns negative thing in C#
		return (x % m + m) % m;
	}

	public void Hurt( float damage )
	{
		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//deal damage
		HP -= damage / defense;
		if ( HP <= 0.0f )
		{
			//deadz.
			//Go into downed state.
			isDowned = true;
			//TODO: deduct points
		}
	}

	public void Wound( float damage )
	{
		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//reduce max hp
		maxHP -= damage;
		if ( maxHP <= 1.0f )
		{
			//having negative max HP makes less than no sense.
			maxHP = 1.0f;
		}
	}

	public void Interrupt( float damage )
	{
		//Attempt to interrupt the current move.
		//(Friendly player attacks you)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//TODO: diminishing returns, thresholds for moves, move interrupt power scaling
		//TODO: on successful interrupt, set canMove to true.
		interruptHP -= damage;
		interruptDR++;

		if ( interruptHP <= 0.0f )
		{
			canMove = true;
			//TODO: reset state stuff
			//TODO: set ani to idle
			//TODO: play interrupt sound
		}
	}

	public void KnockBack( float magnitude, Vector2 pos )
	{
		//Knock the player back? (based on where the attack come from)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//TODO: move knockback power scaling
		//TODO: effects that minimize / reduce knockback. (blocking, parrying, dodging?)
		//TODO: interrupt stuff on knockback?
	}

	//-------------------------------------------------------------------------------------------------------
	//ITEMS?
	//-------------------------------------------------------------------------------------------------------
	public void UseItem()
	{
		if ( items[ itemIndex ].CoolDownTimer <= 0.0f )
		{
			//TODO: check cooldowns, set cooldowns on use.
			StopWatch();
			items[ itemIndex ].CoolDownTimer = items[ itemIndex ].CoolDownDelay;
		}
		else
		{
			//Still on cooldown.
		}
	}

	private void StopWatch()
	{
		isInBulletTime = true;
		bulletTimeDuration = 30.0f;
		StaticData.t_scale = 0.5f;
		StaticData.bulletTimeDuration = bulletTimeDuration;
		//TODO: lerp in, add visual effect, play sound
		//TODO: duration on t scale, lerp back in, remove visual effect, play sound
	}
}
