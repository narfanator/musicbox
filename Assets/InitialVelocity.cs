using UnityEngine;
using System.Collections;

public class InitialVelocity : MonoBehaviour {
    public float v = 1;
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, v, 0);
	}
	
	// Update is called once per frame
	void Update () {
	}
}
