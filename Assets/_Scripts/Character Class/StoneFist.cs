﻿using UnityEngine;
using System.Collections;

public class StoneFist : MonoBehaviour, ClassFunctionalityInterface 
{

	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";
	#endregion
	
	// Use this for initialization
	void Start () 
	{
		player = GetComponent<Player>();
		controller = GetComponent<CustomController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//TODO: trap state changes
		if ( prevState != player.state )
		{
			OnStateChange( player.state );
		}
		prevState = player.state;
	}

	#region B
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
		//TODO: stone skin (parry)
	}
	public void BReleased()
	{
		//Called when B is released.
	}
	public void BHeld( float dt )
	{
		//Called every frame B is held down.
	}
	#endregion
	
	#region X
	//Tap / Hold
	public void XPressed()
	{
		//Called when X is pressed.
	}
	public void XReleased()
	{
		//Called when X is released.
		//TODO: sand laser
	}
	public void XHeld( float dt )
	{
		//Called every frame X is held down.
		//TODO: charge sand laser
	}
	public void XRest( float dt )
	{
		//Called every frame X is in it's natural state.
	}
	#endregion
	
	#region Y
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
		//TODO: monodirectional shield
	}
	public void YReleased()
	{
		//Called when Y is released.
		//TODO: counterattack
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
		//TODO: charge up counterattack
	}
	public void YRest( float dt )
	{
		//Called every frame Y is in it's natural state.
	}
	#endregion
	
	#region RT
	//Tap / Hold
	public void RTPressed()
	{
		//Called when RT is pressed.
		//TODO: sandstorm
	}
	public void RTReleased()
	{
		//Called when RT is released.
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
	}
	#endregion

	void OnStateChange( string newState )
	{
		//handles setting up chains of state changes.
	}
}