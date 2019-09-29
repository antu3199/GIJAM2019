using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClashEventModule : MonoBehaviour, PlayerControls {
	
	//Input
	public string up, down, left, right;
	public string horizontal, vertical;
	public float m_comboDamage;
	public float m_comboDefense;

	int command;
	bool playerInfluence;

	//Disable/Enable Player Controls without disabling physics
    public void DisablePlayerInfluence()
    {
    	playerInfluence = false;
    }

	public void EnablePlayerInfluence()
	{
		playerInfluence = true;
	}

	// [0 , 1 , 2 , 3] -> [L , U , R , D]
	public int GetCommand() {
		return command;
	}

	public void ResetCommand() {
		command = -1;
	}

	// Use this for initialization
	void Start () {
		command = -1;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(left) || Input.GetAxis(vertical) > 0) {
			command = 0;
		}
		else if(Input.GetKey(up) || Input.GetAxis(horizontal) > 0) {
			command = 1;
		}
		else if(Input.GetKey(right) || Input.GetAxis(vertical) < 0) {
			command = 2;
		}
		else if(Input.GetKey(down) || Input.GetAxis(horizontal) < 0) {
			command = 3;
		}
	}
}
