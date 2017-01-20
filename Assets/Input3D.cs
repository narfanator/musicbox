using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/* This is an input module for turning a list of spherical colliders
 * into "input objects" that can interact with the Unity UI system.
 * 
 * TODO: Make off-camera interactions happen.
 */

public class Input3D : BaseInputModule {
    public static List<VRInput2> pointers = new List<VRInput2>();

    void updateObjectsList() {
        List<GameObject> enteredObjects = new List<GameObject>();

        foreach (VRInput2 vrPointer in pointers) {
            GameObject pointer = vrPointer.gameObject;

            //TODO: Cache value?
            float pointerDepth = pointer.transform.lossyScale.z * pointer.GetComponent<SphereCollider>().radius;

            //Note: Only called for the activated module
            Vector3 pointerPos = Camera.main.WorldToScreenPoint(pointer.transform.position);
            PointerEventData pointerData = new PointerEventData(EventSystem.current) {
                pointerId = pointers.IndexOf(vrPointer) + 1, //Zero is for the mouse. TODO: Better enumeration?
                position = pointerPos,
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            //We only have to worry about 2d objects, here; 3D objects are handled by colliders
            foreach (RaycastResult res in results) {
                Vector3 relativeVector = res.gameObject.transform.position - pointer.transform.position;
                float distanceFromPlane = Vector3.Dot(res.gameObject.transform.forward, relativeVector);

                if (Mathf.Abs(distanceFromPlane) < pointerDepth) {
                    enteredObjects.Add(res.gameObject);
                    ExecuteEvents.ExecuteHierarchy(res.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
                } else if (enteredObjects.Contains(res.gameObject)) {
                    enteredObjects.Remove(res.gameObject);
                    ExecuteEvents.ExecuteHierarchy(res.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
                }
            }

            vrPointer.pointerData = pointerData;
            vrPointer.enteredObjects = enteredObjects;
        }
    }

    public override void Process() {
        updateObjectsList();


    }
}
