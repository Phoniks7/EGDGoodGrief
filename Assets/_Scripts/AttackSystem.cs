﻿using UnityEngine;
using System.Collections;

//ATTACK SYSTEM CLASS:
//basically, you input a shape, the amount of damage, and an ID, 
//and it damages all appropriate objects that it intersects.

//TODO: friendly fire flag?, damage attenuation?

public enum PLAYER_IDS { BOSS = -1, ONE = 0, TWO = 1, THREE = 2, FOUR = 3 }; //makes id codes make english sense.

public static class AttackSystem 
{
	public static void hitCircle( Vector2 center, float radius, float damage, int id )
	{
		//TODO: layer mask based on id
		Collider2D[] hits = Physics2D.OverlapCircleAll( center, radius );

		foreach ( Collider2D hit in hits )
		{
			Hit ( hit.gameObject, id, damage );
		}
	}

	public static void hitLineSegment( Vector2 start, Vector2 end, float damage, int id )
	{
		//TODO: layer mask based on id

		#region DEBUG
		Debug.DrawLine ( start, end, new Color( 0.0f, 1.0f, 0.0f) );
		#endregion

		RaycastHit2D[] hits = Physics2D.LinecastAll( start, end );

		foreach ( RaycastHit2D hit in hits )
		{
			Hit ( hit.collider.gameObject, id, damage );
		}
	}

	public static void hitBox( Rect attackBox, float damage, int id )
	{
		#region DEBUG
		Debug.DrawLine ( new Vector3( attackBox.x, attackBox.y ), 
		                 new Vector3( attackBox.x + attackBox.width, attackBox.y ), 
		                 new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x + attackBox.width, attackBox.y ), 
		                new Vector3( attackBox.x + attackBox.width, attackBox.y + attackBox.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x + attackBox.width, attackBox.y + attackBox.height ), 
		                new Vector3( attackBox.x, attackBox.y + attackBox.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x, attackBox.y + attackBox.height ), 
		                new Vector3( attackBox.x, attackBox.y ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		#endregion

		//TODO: layer mask based on id
		Collider2D[] hits = Physics2D.OverlapAreaAll( new Vector2( attackBox.x, attackBox.y ), 
		                                              new Vector2( attackBox.x + attackBox.width, attackBox.y + attackBox.height) );
		foreach ( Collider2D hit in hits )
		{
			Hit ( hit.gameObject, id, damage );
		}
	}
	
	public static void hitSector( Vector2 pos, float minTheta, float maxTheta, float maxRadius, float damage, int id )
	{
		//TODO: layer mask this!
		
		//NOTE: theta is an angle, in DEGREES, >= -360.0f
		float minRadius = 0.0f;
		
		#region DEBUG
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		            pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		            0.0f ), 
		               new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		            pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		            0.0f ), 
		               new Color( 0.0f, 1.0f, 0.0f ) );
		#endregion
		
		//get all in circle, get angle from center to center, check if in range.
		//if not in angle range, check if linecast hits
		
		//if getcomponent player != null, do player stuff
		//if getcomponent boss != null, do boss stuff
		Collider2D[] potentialHits = Physics2D.OverlapCircleAll( pos, maxRadius );
		foreach (Collider2D hit in potentialHits )
		{
			//COMMON LOGIC
			float x = hit.gameObject.transform.position.x;
			float y = hit.gameObject.transform.position.y;

			#region Hit Shape Offsets
			BoxCollider2D tempBox       = hit.GetComponent<BoxCollider2D>();
			CircleCollider2D tempCircle = hit.GetComponent<CircleCollider2D>();
			if ( tempBox != null )
			{
				x += tempBox.center.x;
				y += tempBox.center.y;
			}
			else if ( tempCircle != null )
			{
				x += tempCircle.center.x;
				y += tempCircle.center.y;
			}
			#endregion

			float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;
			Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );
			
			if ( angle >= minTheta && angle <= maxTheta )
			{
				//In sector angle range.
				Debug.Log ( "Hit, normal" );
				Hit( hit.gameObject, id, damage );
			}
			else if ( (minTheta + 360.0f) % 360.0f > (maxTheta + 360.0f) % 360.0f )
			{
				//The sign on the angle changed. 
				//So to tell if it's in the sector, 
				//we check that it is not in the complement of the angle swept from min to max
				//(the complement is the angle swept from max to min.)
				if ( ! (angle >= ( (maxTheta + 360.0f) % 360.0f) && angle <= ( (minTheta + 360.0f) % 360.0f) ) )
				{
					Debug.Log ( "Hit, negative angle" );
					Hit ( hit.gameObject, id, damage );
				}
			}
			else
			{
				//possibly no hit: one last check for edge cases.
				//2 linecasts at the edges of the sector.
				Vector2 start = new Vector2( x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
				                            y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ) );
				Vector2 end   = new Vector2( x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
				                            y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ) );
				RaycastHit2D[] edgeHits1 = Physics2D.LinecastAll ( start, end );
				
				start = new Vector2( x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
				                    y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ) );
				end   = new Vector2( x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
				                    y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ) );
				RaycastHit2D[] edgeHits2 = Physics2D.LinecastAll ( start, end );
				
				//Edge case checks
				bool isHit = false; //to prevent multi-hitting w/ narrow edges.
				foreach ( RaycastHit2D lastHit in edgeHits1 )
				{
					if ( lastHit.collider.gameObject.Equals ( hit.gameObject ) )
					{
						Debug.Log ( "Hit, edge case (min)." ); //does not ignore self hits
						Hit( hit.gameObject, id, damage );
						isHit = true;
					}
				}
				
				if ( isHit == false )
				{
					foreach ( RaycastHit2D lastHit in edgeHits2 )
					{
						if ( lastHit.collider.gameObject.Equals ( hit.gameObject ) )
						{
							Debug.Log ( "Hit, edge case (max)." );
							Hit( hit.gameObject, id, damage );
							isHit = true;
						}
					}
				}
			}
			//END COMMON LOGIC
		}
		
		/*

		#region players
		//players
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				if ( i != id ) //no self-hitting
				{
					float x = GameState.players[i].transform.position.x;
					float y = GameState.players[i].transform.position.y;
					float dist = Mathf.Pow(  ( (pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y) ), 0.5f );

					if ( dist >= minRadius && dist <= maxRadius )
					{
						float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;

						//Debug.Log ( angle ); //DEBUG
						Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );

						if ( angle >= minTheta && angle <= maxTheta )
						{
							//Debug.Log ( "Hit, normal" );
							GameState.players[i].GetComponent<Player>().Hurt ( damage );
						}
						else if ( (minTheta + 360.0f) % 360.0f > (maxTheta + 360.0f) % 360.0f )
						{
							//The sign on the angle changed. 
							//So to tell if it's in the sector, 
							//we check that it is not in the complement of the angle swept from min to max
							//(the complement is the angle swept from max to min.)
							if ( ! (angle >= ( (maxTheta + 360.0f) % 360.0f) && angle <= ( (minTheta + 360.0f) % 360.0f) ) )
							{
								//Debug.Log ( "Hit, negative angle" );
								GameState.players[i].GetComponent<Player>().Hurt ( damage );
							}
						}
					}
				}
			}
		}
		#endregion
		#region boss
		//boss
		if ( GameState.boss != null )
		{
			float x = GameState.boss.transform.position.x;
			float y = GameState.boss.transform.position.y;
			float dist = Mathf.Pow(  ( (pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y) ), 0.5f );
			
			if ( dist >= minRadius && dist <= maxRadius )
			{
				float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;

				Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );
				
				if ( angle >= minTheta && angle <= maxTheta )
				{
					GameState.boss.GetComponent<Boss>().Hurt ( damage );
				}
				else if ( (minTheta + 360.0f) % 360.0f > (maxTheta + 360.0f) % 360.0f )
				{
					//The sign on the angle changed. 
					//So to tell if it's in the sector, 
					//we check that it is not in the complement of the angle swept from min to max
					//(the complement is the angle swept from max to min.)
					if ( ! (angle >= ( (maxTheta + 360.0f) % 360.0f) && angle <= ( (minTheta + 360.0f) % 360.0f) ) )
					{
						//Debug.Log ( "Hit, negative angle" );
						GameState.boss.GetComponent<Boss>().Hurt ( damage );
					}
				}
			}
		}
		#endregion

        */
	}
	
	private static void Hit( GameObject obj, int id, float damage )
	{
		//Does damage to a game object (boss / player)
		//Gets implicit info about sender from id (player or boss?)
		//Player -> boss: damage
		//Boss -> player: damage
		//Player -> player: interrupt
		//Boss -> boss: masked out

		//NOTE: players cannot hit themselves.
		
		Player hitPlayer = obj.GetComponent<Player>();
		Boss hitBoss = obj.GetComponent<Boss>();
		//TODO: put a hit "add / leg / enemy" in here.
		
		if ( hitPlayer != null ) //hit a player
		{
			if ( IsPlayer ( id ) ) //player -> player attack
			{
				if ( hitPlayer.id != id ) //no self-hitting
				{
					hitPlayer.Interrupt( id, damage );
				}
			}
			else if ( IsEnemy( id ) ) //enemy -> player attack
			{
				hitPlayer.Hurt ( damage );
			}
		}
		else if ( hitBoss != null ) //hit the boss
		{
			if ( IsPlayer ( id ) ) //player -> boss attack
			{
				hitBoss.Hurt ( damage );
			}
			//else: ignore it.
		}
	}

	private static bool IsPlayer( int id )
	{
		//checks if the id belongs to a player.
		return ( id >= 0 && id < 4 );
	}

	private static bool IsEnemy( int id )
	{
		//checks if the id belongs to an enemy.
		return ( id == -1 );
	}
}
