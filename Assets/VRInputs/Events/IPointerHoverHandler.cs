using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public interface IPointerHoverHandler : IEventSystemHandler {
    void onPointerHover();
}
