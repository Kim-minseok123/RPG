using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isDragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
        {
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
