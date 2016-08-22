using UnityEngine;
using System.Collections;

public class Wiggle : MonoBehaviour {

	public Vector3 wigglePosition = Vector3.zero;
	public Vector3 wiggleRotation = Vector3.zero;

	public float frequency = 5.0f;

	public bool aim = false;

	public Transform aimTarget;

	private Transform _trans;

	// Use this for initialization
	void Start () {

		_trans = transform;

	}
	
	// Update is called once per frame
	void Update () {

		float sin = Mathf.Sin (Time.time * frequency) * Time.deltaTime;
		Vector3 sinv = new Vector3 (sin, sin, sin);

		_trans.Translate (Vector3.Scale (wigglePosition, sinv));
		_trans.Rotate (Vector3.Scale (wiggleRotation, sinv));

		if (aim) {

			_trans.LookAt (aimTarget);

		}
	 
	}
}
