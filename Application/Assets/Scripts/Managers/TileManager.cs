using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;

    private static TileManager _instance;
    public static TileManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetPopupPrefab()
    {
        return popupPrefab;
    }

    public void CloseCurrentPopup()
    {
        if (FindAnyObjectByType<ModalManager>() != null)
        {
            Destroy(FindAnyObjectByType<ModalManager>().gameObject);
        }
        else
        {
            Debug.LogWarning("No ModalManager found to close.");
        }
    }
}
