using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileGenerator : MonoBehaviour {
	
	public GameObject[] tileObjects;
	private readonly int NumTiles = 16;
	private readonly int NumRows = 4;
	private readonly int NumCols = 4;
	private readonly float TileSpacing = 1.5f;
	
	private readonly Vector3 CamPos = new Vector3(2.25f, 2.25f, -8f);
	private RaycastHit hit;
	
	private GameObject matchOne = null;
	private GameObject matchTwo;
	
	private bool canClick = true;
	
	private int userScore = 0;
	
	public float secondsRemaining = 90;
	private List<Vector3> tilePositions;
	
	public GUISkin egyptSkin;
	
	public Texture2D finishedTexture;
	public Texture2D timeUpTexture;
	
	private bool timerRunning = true;
	
	void Start (){
		Camera.main.transform.position = CamPos;
			
		tilePositions = new List<Vector3>(NumTiles);
		
		for(int row = 0; row < NumRows; row++){
			for(int col = 0; col < NumCols; col++){
				tilePositions.Add( new Vector3(row * TileSpacing, col * TileSpacing, 0f));
			}
		}
		
		tilePositions = shuffle(tilePositions);
		
		for(int i = 0; i < tilePositions.Count; i++){
			Instantiate(tileObjects[i], tilePositions[i], Quaternion.identity);
		}
	}
	
	private void RevealTileOne(){
		matchOne = hit.transform.gameObject;
		RevealTile(matchOne);
	}
	
	private void RevealTileTwo(){
		string fullTileName1 = matchOne.transform.parent.name;
		string fullTileName2 = matchTwo.transform.parent.name;
		
		string tileName1 = fullTileName1.Split('_')[0];
		string tileName2 = fullTileName2.Split('_')[0];		

		RevealTile(matchTwo);

		if( tileName1 == tileName2 ){
			StartCoroutine(RemoveTilesFromGame());
		}
		else{
			StartCoroutine(HideTiles());
		}
	}
	
	private void StopTimer(){
		timerRunning = false;
	}
	
	IEnumerator RemoveTilesFromGame(){
		yield return new WaitForSeconds(2);
		Destroy(matchOne.collider);
		Destroy(matchTwo.collider);
		matchOne = matchTwo = null;
		canClick = true;
		userScore++;
	}
	
	IEnumerator HideTiles(){
		yield return new WaitForSeconds(2);
		HideTile(matchOne);
		HideTile(matchTwo);
		matchOne = matchTwo = null;
		canClick = true;
	}

	private void RevealTile(GameObject tile){
		tile.transform.parent.animation.CrossFade("tileReveal");
	}
	
	private void HideTile(GameObject tile){
		tile.transform.parent.animation.Play("tileHide");
	}
	
	void OnGUI(){
		GUI.skin = egyptSkin;
		
		if(userScore == NumTiles/2){
			canClick = false;
			GUI.Label(new Rect(	Screen.width/2  - finishedTexture.width/4,
								Screen.height/2 - finishedTexture.height/4,
								finishedTexture.width/2, finishedTexture.height/2), finishedTexture);
			StopTimer();
		}

		else if(secondsRemaining <= 0){
			StopTimer();
			canClick = false;
			GUI.Label(new Rect(	Screen.width/2  - timeUpTexture.width/4,
								Screen.height/2 - timeUpTexture.height/4,
								timeUpTexture.width/2, timeUpTexture.height/2), timeUpTexture);
		}

		int roundedSeconds = Mathf.CeilToInt(secondsRemaining);
		
		int minRemaining = roundedSeconds / 60;
		int secRemaining = roundedSeconds % 60;
		
		GUI.Label(new Rect(Screen.width/2 -10, 60 ,100, 20), minRemaining + ":" + (secRemaining < 10 ? "0" : "")  + secRemaining);
	}
	
	private List<T> shuffle<T>(List<T> plist){
		
		if(plist.Count <= 1){
			return plist;
		}
		
		List<T> list = new List<T>();
		for(int i = 0; i < plist.Count; i++){
			list.Add(plist[i]);
		}
		
		for(int i = 0; i < list.Count - 1; i++){
			int randIndex = Random.Range(i + 1, list.Count);
			T vecTemp = list[randIndex];
			list[randIndex] = list[i];			
			list[i] = vecTemp;
		}
		return list;
	}
	
	void Update () {
		
		if(timerRunning){
			secondsRemaining -= Time.deltaTime;
		}
		
		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(canClick && Input.GetButtonDown("Fire1") && Physics.Raycast(camRay, out hit, Mathf.Infinity)){
			if(matchOne == null){
				RevealTileOne();
			}
			else{
				matchTwo = hit.transform.gameObject;
				if(matchOne != matchTwo){
					canClick = false;
					RevealTileTwo();
				}
				else{
					matchTwo = null;
				}
			}
		}
	}
}
