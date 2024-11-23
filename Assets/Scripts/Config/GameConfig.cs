using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
public class GameConfig : ScriptableObject
{
    public static GameConfig Config => ConfigRepository.GetConfig<GameConfig>();

    [Header("Configuration")]
    public GameObject BgTilePrefabs;
    public SC_Bomb BombPrefab;
    public SC_Gem[] GemPrefabs;
    public float BonusAmount = 0.5f;
    public float BombChance = 2f;
    public int DropHeight = 0;
    public float GemSpeed;
    public int MinGemMatchForBombSpawn = 4;

    [Header("Gem Spawning")]
    public float CascadingGemSpawnDelay = 0.2f;

    [Header("Board")]
    public int RowsSize = 7;
    public int ColsSize = 7;

    [Header("Display")]
    public float ScoreUpdateSpeed = 5.0f;
    public string ValueStringFormat = "N0";

    public SC_Gem GetGemPrefab(GlobalEnums.GemType gemType)
    {
        return GemPrefabs.Single(g => g.type == gemType);
    }
}