using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Reflection;

public class MultipleInputModulesHack : BaseInputModule {
    public override bool ShouldActivateModule() {
        return true;
    }

    public override void Process() {

        BaseInputModule[] inputModules = GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in inputModules) {
            if (module == this || ! module.IsActive())
                continue;

            module.Process();
        }
    }

    //TODO: Properly place.
    public static List<GameObject> updateObjectsList(List<GameObject> newObjects, List<GameObject> oldObjects, PointerEventData pointerData) {
        // Collect all the hit objects
        List<GameObject> _enteredObjects = new List<GameObject>();
        foreach (GameObject obj in newObjects) {
            _enteredObjects.Add(obj);

            if (oldObjects.Contains(obj)) {
                ExecuteEvents.ExecuteHierarchy<IPointerHoverHandler>(obj, pointerData, (x, y) => x.onPointerHover());
            } else {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerEnterHandler);
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.initializePotentialDrag);
            }
        }

        // Any objects in the prior list of entered object that are not in the hit list, are exited.
        foreach (GameObject obj in oldObjects) {
            if (!_enteredObjects.Contains(obj)) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerExitHandler);
            }
        }

        //Update for next frame
        return _enteredObjects;
    }
}
