using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;


public class ViveControllerInput : VR3DInput {
    //TODO: Allow configuration (so something else can be the mouse click)
    public const ulong clickInput = SteamVR_Controller.ButtonMask.Trigger;
    public const ulong gripInput = SteamVR_Controller.ButtonMask.Grip;

    SteamVR_TrackedObject trackedObject;
    SteamVR_Controller.Device controller;
    protected override void Start() {
        base.OnEnable();
        trackedObject = cursor.gameObject.GetComponent<SteamVR_TrackedObject>();
        if(trackedObject == null) {
            Debug.LogError("No Vive tracked object on Cursor. Disabling this module");
            enabled = false;
        }
    }

    public override void Process() {
        if(pointerId < 0) {
            pointerId = (int)trackedObject.index;
            if(pointerId >= 0) {
                controller = SteamVR_Controller.Input(pointerId);
            }
            return;
        }

        // VR3DInput.Process handles all the object list updating, and non-button input stuff
        base.Process();

        if (controller.GetPressDown(clickInput)) {
            onPointerDown();
        } else if (controller.GetPress(clickInput)) {
            onPointerHeld(); //TODO: Make sure this chain is accurate.
        } else if (controller.GetPressUp(clickInput)) {
            onPointerUp();
        }
    }

    public List<GameObject> clickCandidates { get; protected set; }
    protected void onPointerDown() {
        clickCandidates = currentObjects;
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.beginDragHandler); //TODO: Is this correct, and why?
        }
    }

    protected void onPointerHeld() {
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.dragHandler);

            if (clickCandidates.Contains(obj)) {
               // ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler); TODO: Correct event. Not sure the drag is done properly either..
            }
        }
    }

    protected void onPointerUp() {
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerUpHandler);

            if (clickCandidates.Contains(obj)) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

}
