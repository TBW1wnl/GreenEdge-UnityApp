using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager : MonoBehaviour
{
    [Header("displays")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("Panels")]
    [SerializeField] public GameObject multipleChoicePanel;
    [SerializeField] public GameObject situationPanel;

    [Header("actions")]
    [Header("Case Multi-Choice")]
    [SerializeField] private Button choice0;
    [SerializeField] private Button choice1;
    [SerializeField] private Button choice2;
    [SerializeField] private Button choice3;

    [Header("Case Situation")]
    [SerializeField] private Slider slider;
    [SerializeField] private Button approach0;
    [SerializeField] private Button approach1;
    [SerializeField] private Button approach2;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
            Debug.Log("Close button event listener added.");
        }
        else
        {
            Debug.LogError("Close button not assigned in EventsManager!");
        }
    }

    public void ShowMultipleChoice(string title, string content)
    {
        UpdateText(title, content);
        multipleChoicePanel.SetActive(true);
        situationPanel.SetActive(false);
    }

    public void ShowSituation(string title, string content)
    {
        UpdateText(title, content);
        multipleChoicePanel.SetActive(false);
        situationPanel.SetActive(true);
    }

    private void UpdateText(string title, string content)
    {
        titleText.text = title;
        contentText.text = content;
    }

    private void ClosePopup()
    {
        Debug.Log("Close button clicked. Closing Event.");
        TileManager.Instance.CloseCurrentEvent();
    }

}
