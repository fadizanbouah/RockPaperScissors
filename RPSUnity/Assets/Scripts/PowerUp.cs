[System.Serializable]
public class PowerUp
{
    public string name;
    public string description;
    public int cost;

    // Future properties: type, modifiers, etc.

    public PowerUp(string name, string description, int cost)
    {
        this.name = name;
        this.description = description;
        this.cost = cost;
    }
}
