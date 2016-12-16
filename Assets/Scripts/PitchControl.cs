using UnityEngine;
using System.Collections;

public class PitchControl : MonoBehaviour {

    public GameObject control;
    public GameObject min;
    public GameObject max;

    public AudioSource sound;

    // Use this for initialization
    void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
        //Ensure that we're between min and max, and on the correct axis
        Vector3 pos = new Vector3(0, 0, control.transform.localPosition.z);
        if(pos.z > max.transform.localPosition.z) {
            pos.z = max.transform.localPosition.z;
        }
        if(pos.z < min.transform.localPosition.z) {
            pos.z = min.transform.localPosition.z;
        }

        control.transform.localPosition = pos;
        control.transform.localEulerAngles = Vector3.zero;

        //Do the control!
        sound.pitch = pos.z - 1; //We picked very convenient numbers in the rest of the control
	}
}
