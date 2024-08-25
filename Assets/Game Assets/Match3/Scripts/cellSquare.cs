using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class cellSquare : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool willDest = false; // Suitable destination?
    public int cellIndex;
    public int rowNumber;
    public int colNumber;
    public bool canLeft = true;
    public bool canRight = true;
    public bool canUp = true;
    public bool canDown = true;
    public int foodItem = -1; // -1:None, 0:Burger, 1:Pizza, 2:Fries, 3:Drink
    private gameMan GameMan;
    public bool isDropping = false;
    public int dropDistance = 0;

    void Start()
    {
        GameMan = GameObject.Find("Canvas").GetComponent<gameMan>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameMan.mouseCellEnter = cellIndex;
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    //    gameController.BeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Handle dragging visuals or actions here if needed
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    //    gameController.EndDrag(this);
    }
}