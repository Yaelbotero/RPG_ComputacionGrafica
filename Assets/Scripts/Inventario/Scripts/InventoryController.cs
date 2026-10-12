using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private UIInventoryPage inventoryUI;

    public int inventorySize = 12;

    private void Start()
    {
        inventoryUI.InitializeInventoryUI(inventorySize);
        inventoryUI.Hide();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryUI.gameObject.activeSelf)
            {
                inventoryUI.Hide();
            }
            else
            {
                inventoryUI.Show();
            }
        }
    }
}
