using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;

    private static TileManager _instance;
    public static TileManager Instance => _instance;
    public int BuildTime = 100;

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

    public void CloseCurrentEvent()
    {
        if (FindAnyObjectByType<EventsManager>() != null)
        {
            Destroy(FindAnyObjectByType<EventsManager>().gameObject);
        }
        else
        {
            Debug.LogWarning("No EventsManager found to close.");
        }
    }
}
