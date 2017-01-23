using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

/* This is an input module for allowing mouse clicks to interact with Unity UI
 * elements, while VR mode is active.
 * It is intended for development use, not production, but ideally functions in both situations.
 */

public class EditorMouse3D : BaseInputModule {

    private List<GameObject> enteredObjects = new List<GameObject>();

    /// <summary>
    ///  CURRENT STEP: Okay, clicking is more complicated than I thought...
    ///  Probably want to turtles-all-the-way this shit.
    ///  Raycasters check for which objects are in and which are out, and let the "event system" know
    ///  Event System then watches for button events...?
    ///  Interpreted events are triggered (drag, click (down/up on the same object within reasonable time / distance), etc)
    /// </summary>
    /// 
    public GameObject handle;
    public GameObject handle2;

    protected override void Awake() {
        Debug.Log(handle.transform.position);
        Debug.Log(handle2.transform.position);
    }

    public override void Process() {
        Vector3 screenPosition = MainGameViewToScreenPoint(Input.mousePosition);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            pointerId = 0,
            position = screenPosition, //Need to use the screen position for the raycast...
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        List<GameObject> objs = (from r in results select r.gameObject).ToList();

        pointerData.position = worldPosition; // But the *world* position for event handling. (WTF, unity?)

        enteredObjects = MultipleInputModulesHack.updateObjectsList(objs, enteredObjects, pointerData);

        DrawDebugLines(worldPosition);

        if (Input.GetMouseButtonDown(0)) {
            foreach (GameObject obj in enteredObjects) {
               ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerDownHandler);
            }
            onPointerDown(worldPosition, pointerData);
        } else if (Input.GetMouseButton(0)) {
            onPointerHeld(worldPosition, pointerData);
        } else if (Input.GetMouseButtonUp(0)) {
            foreach (GameObject obj in enteredObjects) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerUpHandler);
            }
            onPointerUp(worldPosition, pointerData);
        }
    }

    public float clickTimeEpsilon = 0.1f;
    public float clickPositionEpsilon = 0.1f;
    bool dragging = false;
    float pointerDownTime;
    Vector3 pointerDownPos;
    void onPointerDown(Vector3 pos, PointerEventData pointerData) {
        pointerDownTime = Time.time;
        pointerDownPos = pos;
        
        foreach (GameObject obj in enteredObjects) {
            //TODO: Correct place for this event...?
            ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.initializePotentialDrag);
        }
    }
    void onPointerHeld(Vector3 pos, PointerEventData pointerData) {
        //TODO: Drag stuff...?
        float timeDelta = Mathf.Abs(Time.time - pointerDownTime);
        float posDelta = (pos - pointerDownPos).magnitude;

        if(posDelta > clickPositionEpsilon && !dragging) {
            dragging = true;
            foreach (GameObject obj in enteredObjects) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.beginDragHandler);
            }
        } else if(dragging) {
            foreach (GameObject obj in enteredObjects) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.dragHandler);
            }
        }
    }
    void onPointerUp(Vector3 pos, PointerEventData pointerData) {
        if (!dragging) {
            float timeDelta = Mathf.Abs(Time.time - pointerDownTime);
            float posDelta = (pos - pointerDownPos).magnitude;

            if (timeDelta < clickTimeEpsilon && posDelta < clickPositionEpsilon) {
                foreach (GameObject obj in enteredObjects) {
                    ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.pointerClickHandler);
                }
            }
        } else {
            foreach (GameObject obj in enteredObjects) {
                //TODO: Difference between endDrag, and drop?
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.endDragHandler);
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.dropHandler);
            }
        }
    }

    public static Vector2 GetMainGameViewSize() {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }
    public static Vector3 MainGameViewToScreenPoint(Vector2 point) {
        //WARNING: Must have your aspect ratio set to 2:1 for these numbers to work.
        //TODO: Math to determine correct values from the main game view size, vs the screen size; aspect ratio stuff

        //Get the mouse position as a 
        float x = point.x / GetMainGameViewSize().x;
        float y = point.y / GetMainGameViewSize().y;

        // These values are (currently) experimentally determined
        float xlow = 0.14f;
        float xhigh = 0.8f;

        float ylow = 0.34f;
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

        return Camera.main.ViewportToScreenPoint(viewportPoint);
    }

    public static void DrawDebugLines(Vector3 worldPoint) {
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1)));
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 1)));
        Debug.DrawLine(Camera.main.transform.position, worldPoint);
    }
}