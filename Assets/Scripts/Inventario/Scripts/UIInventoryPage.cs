using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Inventory.UI;
using UnityEngine.UI;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField]
     private UIInventoryItem itemPrefab;

     [SerializeField]
     private RectTransform contentPanel;

     //[SerializeField]
    //private Sprite someSprite;

    [SerializeField]
    private UIInventoryDescription ItemDescription;

    [SerializeField]
    private MouseFollower mouseFollower;

     List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

     public Sprite image;
     public int quantity;
     public string tittle, description;

     private void Awake()
    {
        Hide();
        ItemDescription.ResetDescription();
        mouseFollower.Toggle(false);
    }

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
        ItemDescription.SetDescription(image, tittle, description);
    }

    private void HandleshowItemActions(UIInventoryItem item)
    {

    }

    private void HandleEndDrag(UIInventoryItem item)
    {
        mouseFollower.Toggle(false);
    }

    private void HandleSwap(UIInventoryItem item)
    {

    }

    private void HandleBeginDrag(UIInventoryItem item)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(image, quantity);
    }
    

    public void Show()
        {
            gameObject.SetActive(true);
            ItemDescription.ResetDescription();

            listOfUIItems[0].SetData(image, quantity);
        }

    public void Hide()
        {
            DeselectAllItems();
            gameObject.SetActive(false);
        }   

}
