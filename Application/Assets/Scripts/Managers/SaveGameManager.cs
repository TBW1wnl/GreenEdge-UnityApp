using UnityEngine;
using UnityEngine.UI;

public class SaveGameManager : MonoBehaviour
{

    [SerializeField] public Button closeButton;
    [SerializeField] public Button saveButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveAsJson);
        }

    }

    private void ClosePopup()
    {
        GameManager.Instance.CloseCurrentPopup();
    }

    private void SaveAsJson()
    {

    }

    public static async void Save(string json)
    {
        await ApiService.Save(json);
    }
}
