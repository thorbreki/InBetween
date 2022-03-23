[System.Serializable]
public class LevelData
{
    public int totalAmountOfEnemies;
    public float secondsToSpawn;
    public int enemyDamageBoost;
    public float enemySpeedBoost;
    public float enemyHealthBoost;
    public int numOfMaxActiveEnemies;
}


[System.Serializable]
public class ArrayOfLevelData
{
    public LevelData[] arrayOfLevelData;
}
