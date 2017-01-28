using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public abstract class VRInputModule : BaseInputModule {
    public VRInputModule() : base() {
        currentObjects = new List<GameObject>();
        priorObjects = new List<GameObject>();
    }

    public PointerEventData pointerData { get; protected set; }

    public List<GameObject> currentObjects { get; protected set; }
    public List<GameObject> priorObjects { get; protected set; }
    public abstract override void Process();

    public List<GameObject> enteredObjects { get; protected set; }
    public List<GameObject> hoveredObjects { get; protected set; }
    public List<GameObject> exitedObjects { get; protected set; }
    protected void UpdateObjectsLists() {
        enteredObjects = new List<GameObject>();
        hoveredObjects = new List<GameObject>();
        exitedObjects = new List<GameObject>();

        foreach (GameObject obj in currentObjects) {
            if (priorObjects.Contains(obj)) {
                hoveredObjects.Add(obj);
            } else {
                enteredObjects.Add(obj);
            }
        }
        
        foreach (GameObject obj in priorObjects) {
            if (!currentObjects.Contains(obj)) {
                exitedObjects.Add(obj);
            }
        }
    }

    protected abstract List<GameObject> Raycast();

}