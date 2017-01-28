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
    //public static List<VRInput2> pointers = new List<VRInput2>();

    public List<SphereCollider> pointers = new List<SphereCollider>();
    Dictionary<SphereCollider, List<GameObject>> enteredObjectsByPointer = new Dictionary<SphereCollider, List<GameObject>>();

    public override void Process() {

        // Step 1: Make sure every pointer has an entered objects list
        // TODO: If the pointer object is removed, does this all cleanly delete, or not...?
        foreach(SphereCollider pointer in pointers) {
            if(!enteredObjectsByPointer.ContainsKey(pointer)) {
                enteredObjectsByPointer.Add(pointer, new List<GameObject>());
            }
        }

        // Step 2: For each pointer, find the objects we care about
        foreach(SphereCollider pointer in pointers) {
            
            // Perform the ray cast
            Vector3 pointerPos = Camera.main.WorldToScreenPoint(pointer.transform.position);
            PointerEventData pointerData = new PointerEventData(EventSystem.current) {
                pointerId = pointers.IndexOf(pointer) + 1, //Zero is for the mouse. TODO: Better enumeration?
                position = pointerPos,
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // Filter by depth
            float pointerDepth = pointer.transform.lossyScale.z * pointer.radius;
            List<GameObject> enteredObjects = new List<GameObject>();
            foreach(RaycastResult res in results) {

                Vector3 relativeVector = res.gameObject.transform.position - pointer.transform.position;
                float distanceFromPlane = Vector3.Dot(res.gameObject.transform.forward, relativeVector);

                if (Mathf.Abs(distanceFromPlane) < pointerDepth) {
                    enteredObjects.Add(res.gameObject);
                } else if (enteredObjects.Contains(res.gameObject)) {
                    enteredObjects.Remove(res.gameObject);
                }
            }

            // Step 2a:  
            enteredObjectsByPointer[pointer] = MultipleInputModulesHack.updateObjectsList(enteredObjects, enteredObjectsByPointer[pointer], pointerData);
        }
    }


}
