using UnityEngine.EventSystems;
using UnityEngine;

public class HangarSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public Hangar hangar;
	[Range (10, 20)] public int SwipeThresholdRatio = 16;
	private float dragTreshhold;
	private float startPos;
	private float distance;
	private bool swiped;


	private void Start () {
		dragTreshhold = Screen.width / SwipeThresholdRatio;
	}

	public void OnBeginDrag (PointerEventData eventData) {
		startPos = eventData.position.x;
	}

	public void OnDrag (PointerEventData eventData) {
		if (swiped) return;
		distance = startPos - eventData.position.x;
		if (distance < -dragTreshhold) {			
			hangar.SlideLeft ();
			swiped = true;
		} else if (distance > dragTreshhold) {
			hangar.SlideRight ();
			swiped = true;
		}
	}

	public void OnEndDrag (PointerEventData eventData) {
		swiped = false;
	}

}
