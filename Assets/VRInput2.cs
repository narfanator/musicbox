using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VRInput2 : MonoBehaviour {


    private SteamVR_Controller.Device controller;
    private SteamVR_TrackedObject trackedObj;
    private SphereCollider sphereCollider;

    private static bool initializedButtons = false;

    public PointerEventData pointerData;
    public List<GameObject> enteredObjects = new List<GameObject>();

    void Start() {
        Input3D.pointers.Add(this);

        sphereCollider = GetComponent<SphereCollider>(); //TODO: Tricks with reset, execute in edit, etc
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
    }

    void Grab(GameObject other) {
        Release();
        grabbedObject = other.AddComponent<FixedJoint>();
        grabbedObject.connectedBody = this.GetComponent<Rigidbody>();
    }

    void Release() {
        if (grabbedObject) {
            Vector3 v = (transform.position - lastPosition) / Time.deltaTime;
            grabbedObject.GetComponent<Rigidbody>().velocity = v;
        }
        Destroy(grabbedObject);
    }

    private Vector3 lastPosition = Vector3.zero;
    void Update() {

        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            foreach(GameObject obj in enteredObjects) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler);
            }


            Release();
        }
        lastPosition = transform.position;
    }

    void OnTriggerEnter(Collider other) {
        //other.gameObject.SendMessage("onEnter");

        //Debug.Log("Collider enter: " + other.name);
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            //Debug.Log("Enter press down");
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            // Debug.Log("Enter press up");
        }
    }

    private FixedJoint grabbedObject;
    void OnTriggerStay(Collider other) {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            Grab(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other) {
        //Debug.Log("Collider exit: " + other.name);
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            // Debug.Log("Exit press down");
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            //Debug.Log("Exit press up");
        }
    }

}
