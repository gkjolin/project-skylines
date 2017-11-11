﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using Assets.Generation;

public class TimeControl : MonoBehaviour {

	public RectTransform TimeBar;
	public float EnergyLeft = 100;
	public float EnergyUsage = 8;
	public bool Using;
	private bool WasPressed;
	public Camera View;
	public Text Score;
	private float _score;
	public bool Lost = false;
	public Text GameOver;
	public Text RestartBtn;
	private float _targetGameOver, _targetRestart;
	public GameObject PlayerPrefab;

	public void Lose(){
		Lost = true;
		Time.timeScale = .25f;
		_targetGameOver = 1f;
		StartCoroutine (LostCoroutine());
	}

	IEnumerator LostCoroutine (){
		yield return new WaitForSeconds (3 / (1/Time.timeScale) );
		_targetGameOver = 0;
		while (Lost) {
			yield return new WaitForSeconds (1 / (1/Time.timeScale) );
			_targetRestart = 1;
			yield return new WaitForSeconds (1 / (1/Time.timeScale) );
			_targetRestart = 0;

		}
	}

	public void Restart(){
		if (!Lost)
			return;
		StartCoroutine (RestartCoroutine());
	}

	IEnumerator RestartCoroutine(){
		_targetRestart = 0;
		Lost = false;
		Time.timeScale = 1f;
		_score = 0;
		EnergyLeft = 100;
		Destroy (GameObject.FindGameObjectWithTag("Player"));

		GameObject world = GameObject.FindGameObjectWithTag ("World");
		foreach (Transform t in world.transform) {
			t.GetComponent<Chunk> ().Dispose ();
		}

		GameObject go = Instantiate<GameObject>(PlayerPrefab, Vector3.zero, Quaternion.identity);
		go.GetComponent<ShipCollision> ().Control = this.GetComponent<TimeControl> ();
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FollowShip>().TargetShip = go;

		yield return null;
	}

	void Update(){

		if (Lost && Input.GetKeyDown (KeyCode.Space))
			Restart();

		GameOver.color = new Color(GameOver.color.r, GameOver.color.g, GameOver.color.b, Mathf.Lerp (GameOver.color.a, _targetGameOver, Time.deltaTime * 4f * (1/Time.timeScale)));
		RestartBtn.color = new Color(RestartBtn.color.r, RestartBtn.color.g, RestartBtn.color.b, Mathf.Lerp (RestartBtn.color.a, _targetRestart, Time.deltaTime * 4f * (1/Time.timeScale)));

		if (Lost)
			return;

		if(Input.GetKey(KeyCode.Space) && EnergyLeft > 0 && !WasPressed){
			EnergyLeft -= Time.deltaTime * EnergyUsage * (1/Time.timeScale);
			EnergyLeft = Mathf.Clamp (EnergyLeft, 0, 100);
			Using = true;
		}else{
			EnergyLeft += Time.deltaTime * EnergyUsage * .5f;
			EnergyLeft = Mathf.Clamp (EnergyLeft, 0, 100);
			Using = false;
		}
		TimeBar.sizeDelta = Lerp(TimeBar.sizeDelta, new Vector2 (EnergyLeft-.5f, TimeBar.sizeDelta.y), Time.deltaTime * 6f);

		if (Using) {
			Time.timeScale = .35f;
			View.GetComponent<MotionBlur>().enabled = true;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = false;
		} else {
			Time.timeScale = 1f;
			View.GetComponent<MotionBlur>().enabled = false;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = true;
		}

		if(!Using)
			WasPressed = Input.GetKey(KeyCode.Space);

		_score += Time.deltaTime * 8;
		Score.text = ((int) _score).ToString();
	}

	Vector2 Lerp(Vector2 a, Vector2 b, float d){
		return new Vector2 ( Mathf.Lerp(a.x,b.x,d), Mathf.Lerp(b.x,b.y,d) );
	}
}
