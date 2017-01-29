using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class VRMouseInput : VRInputModule {
    //NOTE: Most TODOs mirrored to VR3DInput
    new const int pointerId = 0; //TODO: What should this be, and why?
    const int mouseButton = 0; //TODO: What should this be, and why?
    public float dragThreshold = 0.1f; //TODO: What should this be, and why?
    public override void Process() {

        // Update objects lists
        priorObjects = currentObjects;
        currentObjects = Raycast();
        UpdateObjectsLists();

        pointerData = new PointerEventData(EventSystem.current) {
            pointerId = pointerId,
            scrollDelta = Input.mouseScrollDelta,
            position = MainGameViewToWorldPoint(Input.mousePosition, 10f), //TODO: Should this be a world position, and why? What should the Z be?
            //TODO: What other data fields should be populated in here?
        };

        //Process entering, hovering and exiting
        foreach(GameObject obj in enteredObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.initializePotentialDrag); //TODO: Should this be here, and why?
        }

        foreach (GameObject obj in hoveredObjects) {
            ExecuteEvents.ExecuteHierarchy<IPointerHoverHandler>(obj, pointerData, (x, y) => x.onPointerHover()); //TODO: Is this correct, and why?
        }

        foreach(GameObject obj in exitedObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerExitHandler);
        }

        // Process clicks
        if (Input.GetMouseButtonDown(mouseButton)) {
            onPointerDown();
        } else if (Input.GetMouseButton(mouseButton)) {
            onPointerHeld();
        } else if (Input.GetMouseButtonUp(mouseButton)) {
            onPointerUp();
        }
        
        // Process scrolls
        if (Input.mouseScrollDelta != Vector2.zero) {
            onMouseScroll();
        }
    }
    
    //TODO: May want to move these to base class, as they're shared with ViveControllerInput
    public List<GameObject> clickCandidates { get; protected set; } //TODO: Better name?
    protected void onPointerDown() {
        clickCandidates = currentObjects; //TODO: Copy, or reference? (Probably need to be a reference)
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.beginDragHandler); //TODO: Is this correct, and why?
        }
    }

    protected void onPointerHeld() {
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.dragHandler);

            if (clickCandidates.Contains(obj)) {
                //ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler); //TODO: Incorrect event. (Event needs to be made?)
            }
        }
    }

    protected void onPointerUp() {
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerUpHandler);

            if(clickCandidates.Contains(obj)) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

    protected void onMouseScroll() {
        foreach (GameObject obj in currentObjects) {
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.scrollHandler);
        }
    }

    //TODO: Raycast(vector2 screenPosition)?
    protected override List<GameObject> Raycast() {
        /// TODO: Why does the raycast want to be a screen position, but the pointerData when processing want to be a world position...?

        Vector3 screenPosition = MainGameViewToScreenPoint(Input.mousePosition);

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            pointerId = pointerId,
            position = screenPosition,
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return (from r in results select r.gameObject).ToList();
    }

    // Utility & Debug Functions

    public static void DrawDebugLines(Vector3 worldPoint) {
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, worldPoint);
    }

    public static Vector2 GetMainGameViewSize() {
        //TODO: Cache the reflection...?
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2) Res;
    }
    
    public static Vector3 MainGameViewToWorldPoint(Vector2 point, float depth) {
        Vector3 viewportPoint = MainGameViewToViewportPoint(point);
        viewportPoint.z = depth;
        return Camera.main.ViewportToWorldPoint(viewportPoint);
    }

    public static Vector3 MainGameViewToScreenPoint(Vector2 point) {
        Vector3 viewportPoint = MainGameViewToViewportPoint(point);
        return Camera.main.ViewportToScreenPoint(viewportPoint);
    }

    public static Vector2 MainGameViewToViewportPoint(Vector2 point) {
        //WARNING: Must have your aspect ratio set to 2:1 for these numbers to work.
        //TODO: Math to determine correct values from the main game view size, vs the screen size; aspect ratio stuff

        //Get the mouse position as a percentage of the main game view
        float x = point.x / GetMainGameViewSize().x;
        float y = point.y / GetMainGameViewSize().y;

        // These values are (currently) experimentally determined
        const float xlow    = 0.14f;
        const float xhigh   = 0.8f;
        const float ylow    = 0.34f;
        const float yhigh   = 0.65f;

        //EXCEPT - Experimentally, the game pane is a subset of the viewport, so we've got to map it.
        // Experimentally, you need to map (0.1, 0.3) <-> (0, 0)
        //   and (0.8, 0.7) <-> (0, 0)
        // With math, that gets us this equation:
        return new Vector2(
            (xhigh - xlow) * x + xlow,
            (yhigh - ylow) * y + ylow
            //10 //TODO: What should this be, and why?
        );
    }
}
