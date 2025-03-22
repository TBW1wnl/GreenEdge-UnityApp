using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GeodesicSphere;

public class ModalManager : MonoBehaviour
{
    [Header("displays")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private Image TerrainImage;
    [SerializeField] private TextMeshProUGUI roadInfrastructureText;
    [SerializeField] private TextMeshProUGUI railInfrastructureText;
    [SerializeField] private TextMeshProUGUI airInfrastructureText;
    [SerializeField] private TextMeshProUGUI seaInfrastructureText;

    [Header("sprites")]
    [SerializeField] private Sprite plainsSprite;
    [SerializeField] private Sprite forestSprite;
    [SerializeField] private Sprite hillsSprite;
    [SerializeField] private Sprite mountainsSprite;
    [SerializeField] private Sprite beachSprite;
    [SerializeField] private Sprite oceansSprite;
    [SerializeField] private Sprite citySprite;
    [SerializeField] private Sprite coldSprite;

    [Header("other")]
    [SerializeField] private GameObject notOwnedPanel;
    [SerializeField] private GameObject ownedPanel;

    [Header("notOwnedPanel")]
    [SerializeField] private Button buildButton;

    [Header("ownedPanel")]

    private Tile Tile;
    Dictionary<TerrainType, Sprite> TerrainSprites = new();

    public static bool isModalOpen = false;


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

        if (buildButton != null)
        {
            buildButton.onClick.AddListener(BuildInTile);
            Debug.Log("Build button event listener added.");
        }
        else
        {
            Debug.LogError("Build button not assigned in ModalManager!");
        }

    }

    public void Initialize(Tile tile)
    {
        Tile = tile;
        isModalOpen = true;

        if (titleText != null)
        {
            titleText.text = $"Province name: {tile.tileData.TileName}";
        }

        if (populationText != null)
        {
            populationText.text = $"Population: {tile.tileData.Population}";
        }

        if (TerrainImage != null)
        {
            TerrainSprites.AddRange(
                (TerrainType.Ocean, oceansSprite),
                (TerrainType.Beach, beachSprite),
                (TerrainType.Plains, plainsSprite),
                (TerrainType.Forest, forestSprite),
                (TerrainType.Hills, hillsSprite),
                (TerrainType.City, citySprite),
                (TerrainType.Mountain, mountainsSprite),
                (TerrainType.Cold, coldSprite)
            );
            TerrainImage.sprite = TerrainSprites[tile.tileData.TerrainType];
        }

        if (roadInfrastructureText != null)
        {
            roadInfrastructureText.text = $"Road: {tile.tileData.RoadInfrastructure.Level}/{tile.tileData.RoadInfrastructure.MaxLevel}";
        }

        if (railInfrastructureText != null)
        {
            railInfrastructureText.text = $"Rail: {tile.tileData.RailInfrastructure.Level}/{tile.tileData.RailInfrastructure.MaxLevel}";
        }

        if (airInfrastructureText != null)
        {
            airInfrastructureText.text = $"Air: {tile.tileData.AirInfrastructure.Level}/{tile.tileData.AirInfrastructure.MaxLevel}";
        }

        if (seaInfrastructureText != null)
        {
            seaInfrastructureText.text = $"Sea: {tile.tileData.SeaInfrastructure.Level}/{tile.tileData.SeaInfrastructure.MaxLevel}";
        }

        if (tile.tileData.IsBuild)
        {
            notOwnedPanel.SetActive(false);
            ownedPanel.SetActive(true);
        }
        else
        {
            ownedPanel.SetActive(false);
            notOwnedPanel.SetActive(true);
        }
    }

    private void ClosePopup()
    {
        Debug.Log("Close button clicked. Closing popup.");
        TileManager.Instance.CloseCurrentPopup();

        isModalOpen = false;
    }

    private void BuildInTile()
    {
        Tile.tileData.IsBuild = true;

        notOwnedPanel.SetActive(false);
        ownedPanel.SetActive(true);
    }


}
