using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainScript : MonoBehaviour {

	private bool hasLocation = false;
	private bool timeRunning = false;
	private float time = 0f;

	public GameObject canvas;
	private TextMeshProUGUI buttonTimeText;
	private TextMeshProUGUI positionText;
	private TextMeshProUGUI timeText;
	private TextMeshProUGUI kmText;
	private TextMeshProUGUI velocityText;

	public float kmRefresh = 10f;
	private float currentKmRefresh;
	private double km = 0f;
	private LocationInfo locInfo;

	// Use this for initialization
	IEnumerator Start()
	{

		currentKmRefresh = kmRefresh;

		buttonTimeText = canvas.transform.Find ("ButtonTime/Text").GetComponent<TextMeshProUGUI>();
		positionText = canvas.transform.Find ("Position").GetComponent<TextMeshProUGUI>();
		timeText = canvas.transform.Find ("Time").GetComponent<TextMeshProUGUI>();
		kmText = canvas.transform.Find ("Km").GetComponent<TextMeshProUGUI>();
		velocityText = canvas.transform.Find ("Velocity").GetComponent<TextMeshProUGUI>();

		/*
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser)
			yield break;
		*/

		// Start service before querying location
		Input.location.Start();

		// Wait until service initializes
		float maxWait = 20f;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			maxWait -= Time.deltaTime;
			yield return null;
		}

		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			Debug.Log("Timed out");
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			hasLocation = true;
			locInfo = Input.location.lastData;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (timeRunning) {
			
			time += Time.deltaTime;
			timeText.text = "Tiempo: " + time.ToString ("0.00");

			currentKmRefresh -= Time.deltaTime;
			if (currentKmRefresh < 0f) {
				UpdateKm ();
			}

		}

		if (!hasLocation)
			return;

		positionText.text = "Posición: " + Input.location.lastData.latitude.ToString("0.00") + ", " + Input.location.lastData.longitude.ToString("0.00") + ", " + Input.location.lastData.altitude.ToString("0.00");

		//print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

	}

	private void UpdateKm() {

		currentKmRefresh = kmRefresh;
		km += Haversine(locInfo, Input.location.lastData);
		kmText.text = "Km Recorridos: " + km.ToString ("0.00");
		locInfo = Input.location.lastData;
		velocityText.text = "Velocidad: " + (km / time).ToString ("0.00") + " Km/s";

	}

	private double Haversine(LocationInfo loc1, LocationInfo loc2) {

		var R = 6371e3; // metres
		var φ1 = loc1.latitude * Mathf.Deg2Rad;
		var φ2 = loc2.latitude * Mathf.Deg2Rad;
		var Δφ = (loc2.latitude - loc1.latitude) * Mathf.Deg2Rad;
		var Δλ = (loc2.longitude - loc1.longitude) * Mathf.Deg2Rad;

		var a = Mathf.Sin(Δφ/2f) * Mathf.Sin(Δφ/2f) +
			Mathf.Cos(φ1) * Mathf.Cos(φ2) *
			Mathf.Sin(Δλ/2) * Mathf.Sin(Δλ/2);
		var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f-a));

		var d = R * c;

		return d;

	}

	public void ButtonTimePressed() {

		timeRunning = !timeRunning;

		if (timeRunning) {
			buttonTimeText.text = "Parar";
		} else {
			buttonTimeText.text = "Resumir";
			UpdateKm ();
		}

	}

}
