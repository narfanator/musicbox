using UnityEngine;
using System.Collections;
public class VRInput : MonoBehaviour {

    private SteamVR_Controller.Device controller;
    private SteamVR_TrackedObject trackedObj;

    void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
    }

    void Update() {
        if (controller == null) {
            Debug.Log("Controller not initialized");
            return;
        }
        
    }

    void OnTriggerEnter(Collider other) {

        AudioSource sound = other.gameObject.GetComponent<AudioSource>();
        if(sound != null) {
            sound.Play();
        }

        Debug.Log("Collider enter: " + other.name);
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Enter press down");
            //joint = other.gameObject.AddComponent<FixedJoint>();
            //joint.connectedBody = this.GetComponent<Rigidbody>();
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Enter press up");
            //Destroy(joint);
        }
    }
    
    void OnTriggerStay(Collider other) {
        //Debug.Log("Stay: " + other.name);
        if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Stay press down");
            FixedJoint joint = other.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = this.GetComponent<Rigidbody>();
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Stay press up");
            Destroy(other.gameObject.GetComponent<FixedJoint>());
        }
    }

    void OnTriggerExit(Collider other) {

        AudioSource sound = other.gameObject.GetComponent<AudioSource>();
        if (sound != null) {
            sound.Stop();
        }

        Debug.Log("Collider exit: " + other.name);
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Exit press down");
            //joint = other.gameObject.AddComponent<FixedJoint>();
            //joint.connectedBody = this.GetComponent<Rigidbody>();
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            Debug.Log("Exit press up");
            //Destroy(joint);
        }
    }

}