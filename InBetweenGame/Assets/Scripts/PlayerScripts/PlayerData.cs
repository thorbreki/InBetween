using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int currentLevel; // NOT ZERO INDEXED
    public int prevLevel;
    public int maxHealth;
    public int pistolDamage;
    public int bombDamage;
    public int staminaShieldDamage;
    public float gunShieldParalyzationSeconds;
    public float bombParalyzationSeconds;
    public int maxStamina;
    public int maxGunEnergy;
    public ApplicationManager.LevelFinishedStatus levelFinishedStatus;
}
