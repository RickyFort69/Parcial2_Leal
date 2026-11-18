using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIClickDebug : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (EventSystem.current == null)
        {
            Debug.Log("No hay ningún EventSystem en la escena");
            return;
        }

        var data = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);

        if (hits.Count == 0)
            Debug.Log("Clic: el raycaster no detecta ningún elemento de UI bajo el mouse");
        else
            Debug.Log("Clic sobre: " + hits[0].gameObject.name + " (de " + hits.Count + " elementos)");
    }
}