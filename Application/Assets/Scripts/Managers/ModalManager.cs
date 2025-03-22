using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI roadInfrastructureText;
    [SerializeField] private TextMeshProUGUI railInfrastructureText;
    [SerializeField] private TextMeshProUGUI airInfrastructureText;
    [SerializeField] private TextMeshProUGUI seaInfrastructureText;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
            Debug.Log("Close button event listener added.");
        }
        else
        {
            Debug.LogError("Close button not assigned in ModalManager!");
        }
    }

    public void Initialize(Tile tile)
    {
        if (titleText != null)
        {
            titleText.text = $"Terrain: {tile.terrainType}, Country ID: {tile.countryId}";
        }

        if (populationText != null)
        {
            populationText.text = $"Population: {tile.population}";
        }

        if (roadInfrastructureText != null)
        {
            roadInfrastructureText.text = $"Road: {tile.RoadInfrastructure.Level}/{tile.RoadInfrastructure.MaxLevel}";
        }

        if (railInfrastructureText != null)
        {
            railInfrastructureText.text = $"Rail: {tile.RailInfrastructure.Level}/{tile.RailInfrastructure.MaxLevel}";
        }

        if (airInfrastructureText != null)
        {
            airInfrastructureText.text = $"Air: {tile.AirInfrastructure.Level}/{tile.AirInfrastructure.MaxLevel}";
        }

        if (seaInfrastructureText != null)
        {
            seaInfrastructureText.text = $"Sea: {tile.SeaInfrastructure.Level}/{tile.SeaInfrastructure.MaxLevel}";
        }
    }

    private void ClosePopup()
    {
        Debug.Log("Close button clicked. Closing popup.");
        TileManager.Instance.CloseCurrentPopup();
    }
}
