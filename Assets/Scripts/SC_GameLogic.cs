using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_GameLogic : Singleton<SC_GameLogic>
{
    public UnityEvent EvtGameStarted { get; private set; } = new();
    public UnityEvent<int> EvtScoreUpdated { get; private set; } = new();

    [Header("References")]
    public Transform GemsContainer;

    private GameBoard gameBoard;
    private ObjectPooling objectPooling;
    private GameConfig config;

    private Vector2Int userLastMovedPosition;
    private GlobalEnums.GemType userLastMovedType;
    private bool shouldSpawnBomb = false;

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
        gameBoard = new GameBoard(7, 7);

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
                GameObject _bgTile = Instantiate(config.BgTilePrefabs, _pos, Quaternion.identity);
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

    private void SpawnGem(Vector2Int _Position, SC_Gem _GemToSpawn, GlobalEnums.GemType? overrideGemType = null)
    {
        SC_Gem _gem = objectPooling.GetPooledObject<SC_Gem>(_GemToSpawn);
        _gem.transform.SetPositionAndRotation(new Vector3(_Position.x, _Position.y + config.DropHeight, 0f),
                                              Quaternion.identity);
        
        _gem.transform.SetParent(GemsContainer);
        _gem.name = "Gem - " + _Position.x + ", " + _Position.y;
        gameBoard.SetGem(_Position.x, _Position.y, _gem);

        if (overrideGemType.HasValue)
        {
            _gem.type = overrideGemType.Value;
        }

        _gem.SetupGem(this, _Position);
        _gem.gameObject.SetActive(true);
    }

    public void SetGem(int _X, int _Y, SC_Gem _Gem)
    {
        gameBoard.SetGem(_X, _Y, _Gem);
    }

    public SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }

    public void UserLastInputData(Vector2Int _position, GlobalEnums.GemType _gemType)
    {
        userLastMovedPosition = _position;
        userLastMovedType = _gemType;
    }

    public void SetState(GlobalEnums.GameState _currentState)
    {
        CurrentState = _currentState;
    }

    public void DestroyMatches()
    {
        CheckLastUserInput();

        for (int i = 0; i < gameBoard.CurrentMatches.Count; i++)
        {
            if (gameBoard.CurrentMatches[i] != null)
            {
                ScoreCheck(gameBoard.CurrentMatches[i]);
                DestroyMatchedGemsAt(gameBoard.CurrentMatches[i].posIndex);
            }
        }

        StartCoroutine(PostDestroyMatchCO());
    }

    private IEnumerator PostDestroyMatchCO()
    {
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(SpawnBombCO());

        StartCoroutine(DecreaseRowCo());
    }

    private void CheckLastUserInput()
    {
        foreach (KeyValuePair<GlobalEnums.GemType, int> comboCounter in gameBoard.CurrentMatchesComboCounter)
        {
            if (userLastMovedType == comboCounter.Key && comboCounter.Value >= config.MinGemMatchForBombSpawn)
            {
                shouldSpawnBomb = true;
                return;
            }
        }

        shouldSpawnBomb = false;
    }

    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.2f);

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
                }
            }
            nullCounter = 0;
        }
        
        StartCoroutine(FilledBoardCo());
    }

    public void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.scoreValue;
        EvtScoreUpdated.Invoke(gameBoard.Score);
    }

    private void DestroyMatchedGemsAt(Vector2Int _Pos)
    {
        SC_Gem _curGem = gameBoard.GetGem(_Pos.x, _Pos.y);
        if (_curGem != null)
        {
            Instantiate(_curGem.destroyEffect, new Vector2(_Pos.x, _Pos.y), Quaternion.identity);

            _curGem.gameObject.GetComponent<Poolable>().Pool();
            SetGem(_Pos.x, _Pos.y, null);
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
        if (shouldSpawnBomb == false) yield break;

        SpawnGem(userLastMovedPosition, config.BombPrefab, userLastMovedType);
        yield return new WaitForSeconds(config.CascadingGemSpawnDelay);
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
                    yield return new WaitForSeconds(config.CascadingGemSpawnDelay);
                }
            }
        }

        CheckMisplacedGems();
    }

    private void CheckMisplacedGems()
    {
        List<SC_Gem> foundGems = new List<SC_Gem>();
        foundGems.AddRange(FindObjectsOfType<SC_Gem>());
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x, y);
                if (foundGems.Contains(_curGem))
                    foundGems.Remove(_curGem);
            }
        }

        //Debug.Log($"[GameLogic] Found {foundGems.Count} misplaced gems!");
        foreach (SC_Gem g in foundGems)
            Destroy(g.gameObject);
    }

    public void FindAllMatches()
    {
        gameBoard.FindAllMatches();
    }

    #endregion
}
