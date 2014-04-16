﻿using UnityEngine;
using System.Collections;

public static class ScoreManager
{
	//A class to neatly contain all the score manipulation code.
	//Interfaces with the players array in gamestate.
	public static void Death( int id )
	{
		//Take 25% of thier points.
		float amount = GameState.players[id].GetComponent<Player>().score * -0.25f;
		GameState.players[id].GetComponent<Player>().score += amount;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.downs.count++;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.downs.score += amount;
	}

	public static void DealtDamage( int id, float damage )
	{
		//give them points ~ damage
		float amount = damage;
		GameState.players[id].GetComponent<Player>().score += amount;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageDealt.count += damage;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageDealt.score += amount;
	}

	public static void TookDamage( int id, float damage )
	{
		//take away points (~ damage?)
		float amount = -1.0f * damage;
		GameState.players[id].GetComponent<Player>().score += amount;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageTaken.count += damage;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageTaken.score += amount;
	}

	public static void AvoidedDamage( int id, float damage )
	{
		//give them points ~ damage
		float amount = damage / 2.0f;
		GameState.players[id].GetComponent<Player>().score += amount;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageAvoided.count += damage;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.damageAvoided.score += amount;
	}

	//BONUS OBJECTIVES
	public static void LastHit( int id )
	{
		//bonus points for last hitting boss.
		float amount = 1000.0f;
		GameState.players[id].GetComponent<Player>().score += amount;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.lastHit.count ++;
		GameState.players[id].GetComponent<Player>().scorePasser.scoreDetails.lastHit.score += amount;
	}

	public static void KilledLeg( int id )
	{
		//bonus points for last hitting leg.
		GameState.players[id].GetComponent<Player>().score += 25.0f;
	}

	public static void BrokeArmor( int id )
	{
		//bonus points for last hitting leg armor.
		GameState.players[id].GetComponent<Player>().score += 5.0f;
	}

	//TODO: helped revive?
	//TODO: killing adds
	//TODO: unity participation / helping bonus
	//TODO: dissension bonus for not getting screwed by the dissension attack.

	//TODO: "GLORIOUS" indicator.
	public static void UpdateScores()
	{
		float sum = 0.0f;
		float mean = 0.0f;
		float variance = 0.0f;
		float stdev = 0.0f;
		for ( int i = 0; i < 4; i++ )
		{
			sum += GameState.players[i].GetComponent<Player>().score;
		}
		mean = sum / 4; //TODO: base on # of players

		//Calculate the variance
		for ( int i = 0; i < 4; i++ )
		{
			float difference = ( GameState.players[i].GetComponent<Player>().score - mean );
			variance += difference * difference;
		}
		variance = variance / 4; //TODO: base on # of players

		//Calculate the standard deviation
		stdev = Mathf.Pow ( variance, 0.5f );

		//calculate player's glory ( or # of standard deviations from the mean)
		//TODO: exponential distribution rather than a linear one.
		for ( int i = 0; i < 4; i++ )
		{
			float glory = 0.0f;
			if ( stdev != 0.0f ) { glory = (GameState.players[i].GetComponent<Player>().score - mean) / stdev; }
			GameState.players[i].GetComponent<Player>().glory = glory;
			//Debug.Log ( glory );
		}
	}
}
