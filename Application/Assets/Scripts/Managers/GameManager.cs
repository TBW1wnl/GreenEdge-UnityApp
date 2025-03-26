using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static TileManager _instance;
    public static TileManager Instance => _instance;

    [SerializeField] private GameObject popupPrefab;
    public void CloseCurrentPopup()
    {
        if (FindAnyObjectByType<SaveGameManager>() != null)
        {
            Destroy(FindAnyObjectByType<SaveGameManager>().gameObject);
        }
    }
}
