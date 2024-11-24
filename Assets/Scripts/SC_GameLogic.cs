using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SC_GameLogic : Singleton<SC_GameLogic>
{
    public class UserLastMovedData
    {
        public Vector2Int LastPosition;
        public GlobalEnums.GemType Type;
        public bool ShouldSpawnBomb = false;

        public UserLastMovedData(Vector2Int _lastPosition, GlobalEnums.GemType _type)
        {
            LastPosition = _lastPosition;
            Type = _type;
        }
    }

    public UnityEvent EvtGameStarted { get; private set; } = new();
    public UnityEvent<int> EvtScoreUpdated { get; private set; } = new();

    [Header("References")]
    public Transform GemsContainer;

    private GameBoard gameBoard;
    private ObjectPooling objectPooling;
    private GameConfig config;

    private UserLastMovedData[] userLastInputData = null;

    public GlobalEnums.GameState CurrentState { get; private set; }

    #region MonoBehaviour
    protected override void Awake()
    {
        base.Awake();

        config = GameConfig.Config;
        objectPooling = ObjectPooling.Instance;

        Init();
    }

    private void Start()
    {
        StartGame();
    }

    #endregion

    #region Logic
    private void Init()
    {
        gameBoard = new GameBoard(config.RowsSize, config.ColsSize);

        for (int i = 0; i < config.GemPrefabs.Length; i++)
        {
            objectPooling.RegisterPrefab(config.GemPrefabs[i].gameObject);
        }

        objectPooling.RegisterPrefab(config.BombPrefab.gameObject);

        Setup();
    }

    private void Setup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Instantiate(config.BgTilePrefab, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(GemsContainer);
                _bgTile.name = "BG Tile - " + x + ", " + y;

                GlobalEnums.GemType _gemToUse = (GlobalEnums.GemType)Random.Range(0, (int)GlobalEnums.GemType.Length);

                int iterations = 0;
                while (gameBoard.MatchesAt(new Vector2Int(x, y), config.GetGemPrefab(_gemToUse)) && iterations < 100)
                {
                    _gemToUse = (GlobalEnums.GemType)Random.Range(0, (int)GlobalEnums.GemType.Length);
                    iterations++;
                }

                SpawnGem(new Vector2Int(x, y), config.GetGemPrefab(_gemToUse));
            }
        }
    }

    public void StartGame()
    {
        CurrentState = GlobalEnums.GameState.Move;
        EvtGameStarted.Invoke();
    }

    private void SpawnGem(Vector2Int _position, SC_Gem _gemToSpawn, GlobalEnums.GemType? _overrideGemType = null)
    {
        SC_Gem _gem = objectPooling.GetPooledObject<SC_Gem>(_gemToSpawn);
        _gem.transform.SetPositionAndRotation(new Vector3(_position.x, _position.y + config.DropHeight, 0f),
                                              Quaternion.identity);

        string name = _gem is SC_Bomb ? "Bomb" : "Gem";
        _gem.transform.SetParent(GemsContainer);
        _gem.name = $"{name} - " + _position.x + ", " + _position.y;
        gameBoard.SetGem(_position.x, _position.y, _gem);

        if (_overrideGemType.HasValue)
        {
            _gem.type = _overrideGemType.Value;
        }

        _gem.SetupGem(this, _position);
        _gem.gameObject.SetActive(true);
    }

    public void SetGem(int _x, int _y, SC_Gem _gem)
    {
        gameBoard.SetGem(_x, _y, _gem);
    }

    public SC_Gem GetGem(int _x, int _y)
    {
        return gameBoard.GetGem(_x, _y);
    }

    public void UserLastInputData(params UserLastMovedData[] _inputData)
    {
        userLastInputData = _inputData;
    }

    public void SetState(GlobalEnums.GameState _currentState)
    {
        CurrentState = _currentState;
    }

    public void DestroyMatches()
    {
        CheckLastUserInput();
        StartCoroutine(DestroyMatchesCO());
    }

    private IEnumerator DestroyMatchesCO()
    {
        DestroyGems(gameBoard.CurrentMatches);
        yield return new WaitForSeconds(config.BombBlastDelay);

        DestroyGems(gameBoard.CurrentBombBlastMatches);
        yield return new WaitForSeconds(config.BombDelay);

        DestroyGems(gameBoard.CurrentBombMatches);
        yield return new WaitForSeconds(config.SpawnDelayAfterDestruction);

        gameBoard.CleanBoard();
        yield return null;

        yield return StartCoroutine(SpawnBombCO());
        yield return null;

        gameBoard.CleanBoard();
        yield return null;

        StartCoroutine(DecreaseRowCO());
    }

    private void DestroyGems(IReadOnlyList<SC_Gem> gems)
    {
        for (int i = 0; i < gems.Count; i++)
        {
            if (gems[i] != null)
            {
                Vector2Int posIndex = gems[i].posIndex;
                if (gameBoard.GetGem(posIndex.x, posIndex.y) == null) continue;

                ScoreCheck(gems[i]);
                DestroyMatchedGemsAt(posIndex);
            }
        }
    }

    private void CheckLastUserInput()
    {
        if (userLastInputData == null || userLastInputData.Length == 0) return;

        foreach (KeyValuePair<GlobalEnums.GemType, int> comboCounter in gameBoard.CurrentMatchesComboCounter)
        {
            for (int i = 0; i < userLastInputData.Length; i++)
            {
                if (userLastInputData[i].Type == comboCounter.Key && comboCounter.Value >= config.MinGemMatchForBombSpawn)
                {
                    userLastInputData[i].ShouldSpawnBomb = true;
                    return;
                }
            }
        }
    }

    private IEnumerator DecreaseRowCO()
    {
        yield return null;

        int nullCounter = 0;
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (_curGem == null)
                {
                    nullCounter++;
                }
                else if (nullCounter > 0)
                {
                    _curGem.posIndex.y -= nullCounter;
                    SetGem(x, y - nullCounter, _curGem);
                    SetGem(x, y, null);

                    yield return new WaitForSeconds(config.DescendGemCascadeSpawnDelay);
                }
            }
            nullCounter = 0;
        }

        StartCoroutine(FilledBoardCo());
    }

    public void ScoreCheck(SC_Gem _gemToCheck)
    {
        gameBoard.Score += _gemToCheck.scoreValue;
        EvtScoreUpdated.Invoke(gameBoard.Score);
    }

    private void DestroyMatchedGemsAt(Vector2Int _pos)
    {
        SC_Gem _curGem = gameBoard.GetGem(_pos.x, _pos.y);
        if (_curGem != null)
        {
            _curGem.Despawn();
            SetGem(_pos.x, _pos.y, null);
        }
    }

    private IEnumerator FilledBoardCo()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(RefillBoard());
        yield return new WaitForSeconds(0.5f);
        gameBoard.FindAllMatches();
        if (gameBoard.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            CurrentState = GlobalEnums.GameState.Move;
        }
    }

    private IEnumerator SpawnBombCO()
    {
        if (userLastInputData == null || userLastInputData.Length == 0) yield break;

        for (int i = 0; i < userLastInputData.Length; i++)
        {
            if (userLastInputData[i].ShouldSpawnBomb)
            {
                SpawnGem(userLastInputData[i].LastPosition, config.BombPrefab, userLastInputData[i].Type);
                yield return new WaitForSeconds(config.NewGemCascadeSpawnDelay);
            }
        }

        userLastInputData = null;
    }

    private IEnumerator RefillBoard()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (_curGem == null)
                {
                    GlobalEnums.GemType? selectedGemType = null;
                    System.Random rand = new();
                    int length = (int)GlobalEnums.GemType.Length;
                    for (int i = length - 1; i > 0; i--)
                    {
                        GlobalEnums.GemType gemType = (GlobalEnums.GemType)rand.Next(i + 1);

                        if (gameBoard.IsTypeMatch(x, y, gemType) == false)
                        {
                            selectedGemType = gemType;
                            break;
                        }
                    }

                    if (selectedGemType == null)
                    {
                        selectedGemType = (GlobalEnums.GemType)Random.Range(0, length);
                        Debug.LogError($"[GB] Randomized: {selectedGemType}");
                    }

                    SC_Gem gemPrefab = config.GetGemPrefab(selectedGemType.Value);

                    SpawnGem(new Vector2Int(x, y), gemPrefab);
                    yield return new WaitForSeconds(config.NewGemCascadeSpawnDelay);
                }
            }
        }

        CheckMisplacedGems();
    }

    private void CheckMisplacedGems()
    {
        List<SC_Gem> foundGems = GemsContainer.GetComponentsInChildren<SC_Gem>().ToList();

        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (foundGems.Contains(_curGem))
                    foundGems.Remove(_curGem);
            }
        }

        if (foundGems.Count > 0)
        {
            Debug.LogError($"[GameLogic] Found {foundGems.Count} misplaced gems!");
            foreach (SC_Gem g in foundGems)
            {
                g.GetComponent<Poolable>().Pool();
            }
        }
    }

    public void FindAllMatches()
    {
        gameBoard.FindAllMatches();
    }

    #endregion
}
