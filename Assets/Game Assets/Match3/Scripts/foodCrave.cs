using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class foodCrave : MonoBehaviour
{
    public int currentCrave = 0;
    public GameObject foodParent;
    private List<foodSquare> foods = new List<foodSquare>();
    private RawImage foodCraveImg;

    void OnEnable()
    {
        foods.Clear();
        foreach (Transform child in foodParent.transform)
        {
            foods.Add(child.GetComponent<foodSquare>());
        }
        foodCraveImg = transform.Find("foodCrave").GetComponent<RawImage>();
        foodCraveImg.texture = foods[currentCrave].foodImage;
    }
}
