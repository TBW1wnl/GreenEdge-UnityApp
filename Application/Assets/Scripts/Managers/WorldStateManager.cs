using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    [Range(0, 100)]
    public int GlobalWarming { get; internal set; } = 0;

    private WorldStateManager() { }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncreaseTension(int amount)
    {
        GlobalWarming = Mathf.Clamp(GlobalWarming + amount, 0, 100);
        Debug.Log($"Réchauffement climatique : {GlobalWarming}%");
    }

    public void DecreaseTension(int amount)
    {
        GlobalWarming = Mathf.Clamp(GlobalWarming - amount, 0, 100);
        Debug.Log($"Réchauffement climatique : {GlobalWarming}%");
    }
}
