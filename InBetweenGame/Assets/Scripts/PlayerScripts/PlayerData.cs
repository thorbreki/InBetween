using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int currentLevel; // NOT ZERO INDEXED
    public int prevLevel;
    public int maxHealth;
    public int coins;
    public int pistolDamage;
    public float pistolAccuracy;
    public float pistolCooldown;
    public float bombCooldown;
    public float helperCooldown;
    public int bombDamage;
    public int staminaShieldDamage;
    public float gunShieldParalyzationSeconds;
    public float bombParalyzationSeconds;
    public int maxStamina;
    public int maxGunEnergy;
    public ApplicationManager.LevelFinishedStatus levelFinishedStatus;
}
