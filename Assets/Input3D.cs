using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Input3D : BaseInputModule {
    public List<GameObject> pointers;
    
    public override void ActivateModule() {
        base.ActivateModule();
    }

    public override void UpdateModule() {
        base.UpdateModule();
    }

    private List<GameObject> enteredObjects = new List<GameObject>();
    public override void Process() {
        foreach (GameObject pointer in pointers) {
            //TODO: Cache value?
            float pointerDepth = pointer.transform.lossyScale.z * pointer.GetComponent<SphereCollider>().radius;

            //Note: Only called for the activated module
            Vector3 pointerPos = Camera.main.WorldToScreenPoint(pointer.transform.position);
            PointerEventData pointerData = new PointerEventData(EventSystem.current) {
                pointerId = -1,
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
                    ExecuteEvents.ExecuteHierarchy(res.gameObject, new PointerEventData(eventSystem), ExecuteEvents.pointerEnterHandler);
                } else if (enteredObjects.Contains(res.gameObject)) {
                    enteredObjects.Remove(res.gameObject);
                    ExecuteEvents.ExecuteHierarchy(res.gameObject, new PointerEventData(eventSystem), ExecuteEvents.pointerExitHandler);
                }
            }
        }
    }

    public override void DeactivateModule() {
        base.DeactivateModule();
    }


    //  ExecuteEvents.ExecuteHierarchy(other.gameObject, new BaseEventData(eventSystem), ExecuteEvents.pointerEnterHandler);
}
