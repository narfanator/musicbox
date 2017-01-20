using UnityEngine;
using System.Collections;

public class BouncePlay : MonoBehaviour {
    public NewSound controller;

    public AudioSource source;

    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    void OnCollisionEnter(Collision collision) {
        //Use our source to play our sound
        source.PlayOneShot(source.clip, source.volume);
    }

    public void endDragHandler() {
        Debug.Log("Ending drag");
        transform.parent = controller.soundSpheres.transform;
        controller.spawnNewSphere();
    }
}
