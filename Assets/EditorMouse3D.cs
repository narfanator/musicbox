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

    public override void Process() {
        Vector3 worldPoint = MainGameViewToWorldPoint(Input.mousePosition);

        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            pointerId = 0,
            position = MainGameViewToWorldPoint(Input.mousePosition),
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        List<GameObject> objs = (from r in results select r.gameObject).ToList();

        enteredObjects = MultipleInputModulesHack.updateObjectsList(objs, enteredObjects, pointerData);
    }

    public static Vector2 GetMainGameViewSize() {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }
    public static Vector3 MainGameViewToWorldPoint(Vector2 point) {
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