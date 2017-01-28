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
    ///  pointerEnterHandler - Check (inherited)
    ///  (pointerHoverHandler) - Added (inherited)
    ///  pointerEnterHandler - Check (inherited)
    ///  pointerDownHandler - Check
    ///  pointerUpHandler - Check
    ///  pointerClickHandler - Checkish. Needs to use object lists, not epsilons
    ///  initializePotentialDrag - Check, but not understood?
    ///  beginDragHandler - Checkish...? Use object lists rather than epsilons...?
    ///  dragHandler - Check
    ///  endDragHandler - Check
    ///  dropHandler - Check, but not understood?
    ///  scrollHandler - Shitty, but check, and needs way more testing
    ///  
    /// I think these are keyboard events, so we don't need to worry about them?
    ///  selectHandler
    ///  deselectHandler
    ///  moveHandler
    ///  submitHandler
    ///  cancelHandler
    ///  
    /// Notes:
    /// Currently, clicking is handled by an epsilon detection. The way it's supposed to work is that the pointer gets a "down" over the obj, and then an "up" over the obj.
    /// PointerEventData can store time and scroll delta!

    protected override void Awake() {
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

        // But the *world* position for event handling. (WTF, unity?)
        // Specifically - for sliders. WTF?
        // Note: Thankfully, the Z position is irrellevant.
        // Also, definitely still something wrong - but this is the most correct, and "good enough" for now
        // TODO: Consequences of sending world position as the enter/exit 
        pointerData.position = worldPosition; 

        enteredObjects = MultipleInputModulesHack.updateObjectsList(objs, enteredObjects, pointerData);

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
        
        if (Input.mouseScrollDelta != Vector2.zero) {
            pointerData.scrollDelta = Input.mouseScrollDelta;
            foreach (GameObject obj in enteredObjects) {
                ExecuteEvents.ExecuteHierarchy(obj, pointerData, ExecuteEvents.scrollHandler);
            }
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