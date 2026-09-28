using UnityEngine;

public class ChestClickHandler : MonoBehaviour
{
    [SerializeField] private TreasureManager manager;

    private void OnMouseDown()
    {
        if (manager != null)
        {
            manager.OnChestClicked();
        }
    }
}