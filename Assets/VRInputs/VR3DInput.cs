using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class VR3DInput : VRInputModule {
    public SphereCollider cursor;

    public float dragThreshold = 0.1f;
    
    public override void Process() {
        // Update objects lists
        priorObjects = currentObjects;
        currentObjects = Raycast();
        UpdateObjectsLists();

        pointerData = new PointerEventData(EventSystem.current) {
            pointerId = pointerId,
            position = cursor.transform.position,
        };

        //Process entering, hovering and exiting
        foreach (GameObject obj in enteredObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.initializePotentialDrag); //TODO: Should this be here, and why?
        }

        foreach (GameObject obj in hoveredObjects) {
            ExecuteEvents.ExecuteHierarchy<IPointerHoverHandler>(obj, pointerData, (x, y) => x.onPointerHover()); //TODO: Is this correct, and why?
        }

        foreach (GameObject obj in exitedObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerExitHandler);
        }
        
    }

    //TODO: Rename (or encapsulate) raycast
    //TODO: Probe points along the circle of the shere (right now, it's really just the center of the sphere)
    protected override List<GameObject> Raycast() {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(cursor.transform.position);

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            pointerId = pointerId,
            position = screenPosition,
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        float pointerDepth = cursor.transform.lossyScale.z * cursor.radius;
        List<GameObject> objs = new List<GameObject>();
        foreach(RaycastResult res in results) {
            Vector3 relativeVector = res.gameObject.transform.position - cursor.transform.position;
            float distanceFromPlane = Vector3.Dot(res.gameObject.transform.forward, relativeVector);
            if (Mathf.Abs(distanceFromPlane) < pointerDepth) {
                objs.Add(res.gameObject);
            }
        }

        // Handle physics objects
        //TODO: Is this the right way to do it? Will it cover the expected layer interaction physics stuff? Should anything else be checked in the collider?
        // What about triggers vs non triggers? Also, always triggers on the cursor's collider... not ideal.
        List<Collider> collisions = Physics.OverlapSphere(cursor.transform.position, pointerDepth).ToList();
        foreach(Collider col in collisions) {
            if (col.gameObject != cursor.gameObject) {
                objs.Add(col.gameObject);
            }
        }

        return objs;
    }

    public static void DrawDebugLines(Vector3 worldPoint) {
        Debug.DrawLine(Camera.main.transform.position, worldPoint);
    }
}
