using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField]
     private UIInventoryItem itemPrefab;

     [SerializeField]
     private RectTransform contentPanel;

     //[SerializeField]
    //private Sprite someSprite;

     List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

    public void InitializeInventoryUI(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel, false); 
            listOfUIItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemClicked;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleshowItemActions;

            //uiItem.SetData(someSprite, 1);
        }
    }

    private void DeselectAllItems()
    {
        foreach (UIInventoryItem item in listOfUIItems)
        {
            item.Deselect();
        }
    }

    private void HandleItemClicked(UIInventoryItem item)
    {
        DeselectAllItems();
        item.Select();
    }

    private void HandleshowItemActions(UIInventoryItem item)
    {

    }

    private void HandleEndDrag(UIInventoryItem item)
    {

    }

    private void HandleSwap(UIInventoryItem item)
    {

    }

    private void HandleBeginDrag(UIInventoryItem item)
    {

    }

    public void Show()
        {
            gameObject.SetActive(true);
        }

    public void Hide()
        {
            DeselectAllItems();
            gameObject.SetActive(false);
        }   

}
