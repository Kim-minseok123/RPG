using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnDragEndHandler = null;
    public Action<PointerEventData> OnDragEnterHandler = null;
    public Action<PointerEventData> OnPointerEnterHandler = null;
    public Action<PointerEventData> OnPointerExitHandler = null;

    public void OnPointerClick(PointerEventData eventData)
	{
		OnClickHandler?.Invoke(eventData);
	}

	public void OnDrag(PointerEventData eventData)
    {
		OnDragHandler?.Invoke(eventData);
	}
    public void OnPointerEnter(PointerEventData eventData)
    {

        OnPointerEnterHandler?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitHandler?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnDragEnterHandler?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnDragEndHandler?.Invoke(eventData);
    }
}
