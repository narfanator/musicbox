using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/* This is an input module for allowing mouse clicks to interact with Unity UI
 * elements, while VR mode is active.
 * It is intended for development use, not production, but ideally functions in both situations.
 */

public class EditorMouse3D : BaseInputModule {
    public static Vector2 GetMainGameViewSize() {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    public void DrawDebugLines(Vector3 worldPoint) {
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, worldPoint);
    }

    private List<GameObject> enteredObjects = new List<GameObject>();
    public override void Process() {
        //TODO: Skip conversion to world point, do Pane -> Viewport -> Screen (or better, Pane -> Screen, it's just math).
        //WARNING: Must have your aspect ratio set to 2:1 for these numbers to work.
        //TODO: Math to determine correct values from the main game view size, vs the screen size; aspect ratio stuff

        //Get the mouse position as a 
        float x = Input.mousePosition.x / GetMainGameViewSize().x;
        float y = Input.mousePosition.y / GetMainGameViewSize().y;

        // These values are (currently) experimentally determined
        float xlow  = 0.14f;
        float xhigh = 0.8f;

        float ylow  = 0.34f;
        float yhigh = 0.65f;

        //EXCEPT - Experimentally, the game pane is a subset of the viewport, so we've got to map it.
        // Experimentally, you need to map (0.1, 0.3) <-> (0, 0)
        //   and (0.8, 0.7) <-> (0, 0)
        // With math, that gets us this equation:
        Vector3 viewportPoint = new Vector3(
            (xhigh - xlow) * x + xlow,
            (yhigh - ylow) * y + ylow,
            10
        );

        Vector3 worldPoint = Camera.main.ViewportToWorldPoint(viewportPoint);
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            pointerId = 0,
            position = Camera.main.WorldToScreenPoint(worldPoint),
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if(Input.GetMouseButtonDown(0)) {
            Debug.Log(viewportPoint);
        }

        // Collect all the hit objects
        List<GameObject> _enteredObjects = new List<GameObject>();
        foreach (RaycastResult res in results) {
            _enteredObjects.Add(res.gameObject);

            if (enteredObjects.Contains(res.gameObject)) {
                //TODO: "Hover" event? Note: Hover is not an existing pointer event.
            } else {
                ExecuteEvents.ExecuteHierarchy(res.gameObject, new PointerEventData(eventSystem), ExecuteEvents.pointerEnterHandler);
            }
        }

        // Any objects in the prior list of entered object that are not in the hit list, are exited.
        foreach(GameObject obj in enteredObjects) {
            if (!_enteredObjects.Contains(obj)) {
                ExecuteEvents.ExecuteHierarchy(obj, new PointerEventData(eventSystem), ExecuteEvents.pointerExitHandler);
            }
        }
    }
}