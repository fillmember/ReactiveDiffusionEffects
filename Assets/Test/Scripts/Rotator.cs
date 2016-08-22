using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
	
	public Vector3 rotation = Vector3.zero;

	// Use this for initialization
	void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {

		transform.Rotate ( rotation * Time.deltaTime );
	
	}
}
