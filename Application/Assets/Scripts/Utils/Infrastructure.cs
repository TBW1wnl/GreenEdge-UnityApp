using UnityEngine;

public class Infrastructure
{
    public int Level { get; set; } = 0;
    public int MaxLevel { get; set; } = 0;

    public void Upgrade()
    {
        if (Level < MaxLevel)
        {
            Level++;
        }
    }

    public void Downgrade()
    {
        if (Level >= 0)
        {
            Level--;
        }
    }
}
