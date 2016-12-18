using UnityEngine;
using System.Collections;
public class VRInput : MonoBehaviour {

    private SteamVR_Controller.Device controller;
    private SteamVR_TrackedObject trackedObj;
    private SphereCollider sphereCollider;

    private static bool initializedButtons = false;
    void Awake() {
        if(!initializedButtons) {
            initializedButtons = true;
            foreach(UnityEngine.UI.Button button in FindObjectsOfType<UnityEngine.UI.Button>()) {
                BoxCollider collider = button.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(
                    button.GetComponent<RectTransform>().rect.width,
                    button.GetComponent<RectTransform>().rect.height,
                    0.01f
                );
            }
        }

        sphereCollider = GetComponent<SphereCollider>(); //TODO: Tricks with reset, execute in edit, etc
    }

    void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
    }

    void Grab(GameObject other) {
        return;
        Release();
        grabbedObject = other.AddComponent<FixedJoint>();
        grabbedObject.connectedBody = this.GetComponent<Rigidbody>();
    }

    void Release() {
        return;
        if(grabbedObject) {
            Vector3 v = (transform.position - lastPosition) / Time.deltaTime;
            grabbedObject.GetComponent<Rigidbody>().velocity = v;
        }
        Destroy(grabbedObject);
    }

    private Vector3 lastPosition = Vector3.zero;
    void Update() {
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {

            foreach(Collider obj in Physics.OverlapSphere(sphereCollider.center, sphereCollider.radius)) {
                Debug.Log("Collision on release: " + obj.name);
                UnityEngine.UI.Button button = obj.gameObject.GetComponent<UnityEngine.UI.Button>();
                if(button) {
                    Debug.Log("Button!");
                    button.onClick.Invoke();
                }
            }

            Release();
        }
        lastPosition = transform.position;
    }

    void OnTriggerEnter(Collider other) {
        UnityEngine.UI.Button button = other.gameObject.GetComponent<UnityEngine.UI.Button>();
        if (button) {
            Debug.Log("Button entered: " + button.name);
            button.onClick.Invoke();
        }

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
        if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
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