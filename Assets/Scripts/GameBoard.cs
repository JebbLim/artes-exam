using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GameBoard
{
    #region Variables

    public int Height { get; private set; } = 0;
    public int Width { get; private set; } = 0;
    public int Score { get; set; }

    private SC_Gem[,] allGems;
    private List<SC_Gem> currentMatches = new();
    private List<SC_Bomb> currentBombMatches = new();
    private List<SC_Gem> currentBombBlastMatches = new();
    private Dictionary<GlobalEnums.GemType, int> currentMatchesComboCounter = new();

    public IReadOnlyList<SC_Gem> CurrentMatches => currentMatches;
    public IReadOnlyList<SC_Gem> CurrentBombMatches => currentBombMatches;
    public IReadOnlyList<SC_Gem> CurrentBombBlastMatches => currentBombBlastMatches;
    public IReadOnlyDictionary<GlobalEnums.GemType, int> CurrentMatchesComboCounter => currentMatchesComboCounter;

    #endregion

    public GameBoard(int _width, int _height)
    {
        Height = _height;
        Width = _width;
        allGems = new SC_Gem[Width, Height];
    }

    public void SetGem(int _x, int _y, SC_Gem _gem)
    {
        allGems[_x, _y] = _gem;
    }

    public SC_Gem GetGem(int _x, int _y)
    {
        return allGems[_x, _y];
    }

    public bool MatchesAt(int _x, int _y, GlobalEnums.GemType _gemType)
    {
        if (_x > 1)
        {
            if (allGems[_x - 1, _y].Type == _gemType && allGems[_x - 2, _y].Type == _gemType)
            {
                return true;
            }
        }

        if (_y > 1)
        {
            if (allGems[_x, _y - 1].Type == _gemType && allGems[_x, _y - 2].Type == _gemType)
            {
                return true;
            }
        }

        return false;
    }

    public bool MatchesAt(Vector2Int _positionToCheck, SC_Gem _gemToCheck)
    {
        return MatchesAt(_positionToCheck.x, _positionToCheck.y, _gemToCheck.Type);
    }

    public void FindAllMatches()
    {
        currentMatches.Clear();
        currentBombMatches.Clear();
        currentBombBlastMatches.Clear();
        currentMatchesComboCounter.Clear();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                SC_Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    if (x > 0 && x < Width - 1)
                    {
                        SC_Gem leftGem = allGems[x - 1, y];
                        SC_Gem rightGem = allGems[x + 1, y];
                        //checking no empty spots
                        if (leftGem != null && rightGem != null)
                        {
                            //Match
                            if (leftGem.Type == currentGem.Type && rightGem.Type == currentGem.Type)
                            {
                                currentGem.IsMatch = true;
                                leftGem.IsMatch = true;
                                rightGem.IsMatch = true;
                                currentMatches.Add(currentGem);
                                currentMatches.Add(leftGem);
                                currentMatches.Add(rightGem);
                            }
                        }
                    }

                    if (y > 0 && y < Height - 1)
                    {
                        SC_Gem aboveGem = allGems[x, y - 1];
                        SC_Gem belowGem = allGems[x, y + 1];
                        //checking no empty spots
                        if (aboveGem != null && belowGem != null)
                        {
                            //Match
                            if (aboveGem.Type == currentGem.Type && belowGem.Type == currentGem.Type)
                            {
                                currentGem.IsMatch = true;
                                aboveGem.IsMatch = true;
                                belowGem.IsMatch = true;
                                currentMatches.Add(currentGem);
                                currentMatches.Add(aboveGem);
                                currentMatches.Add(belowGem);
                            }
                        }
                    }
                }
            }
        }

        // Count Combos
        if (currentMatches.Count > 0)
        {
            currentMatches = currentMatches.Distinct().ToList();

            for (int i = 0; i < currentMatches.Count; i++)
            {
                if (currentMatchesComboCounter.ContainsKey(currentMatches[i].Type) == false)
                {
                    currentMatchesComboCounter.Add(currentMatches[i].Type, 1);
                }
                else
                {
                    currentMatchesComboCounter[currentMatches[i].Type]++;
                }
            }

            // Filter out Bomb gems
            for (int i = 0; i < currentMatches.Count; i++)
            {
                SC_Gem gem = currentMatches[i];
                if (gem is not SC_Bomb) continue;

                currentBombMatches.Add(gem as SC_Bomb);
            }

            // Remove Bombs from CurrentMatches
            for (int i = 0; i < currentBombMatches.Count; i++)
            {
                currentMatches.Remove(currentBombMatches[i]);
            }
        }

        if (currentBombMatches.Count > 0)
        {
            CheckForBombBlasts(currentBombMatches);
        }
    }

    private void CheckForBombBlasts(List<SC_Bomb> bombs)
    {
        List<SC_Gem> bombBlastMatches = new();
        List<SC_Bomb> additionalBombMatches = new();

        for (int i = 0; i < bombs.Count; i++)
        {
            SC_Bomb bomb = bombs[i];

            IEnumerable<SC_Gem> markArea = MarkBombArea(bomb.PosIndex, bomb.BlastSize);
            bombBlastMatches.AddRange(markArea);

            foreach (SC_Gem gem in bombBlastMatches)
            {
                if (gem is SC_Bomb)
                {
                    additionalBombMatches.Add(gem as SC_Bomb);
                }
            }
        }

        currentBombBlastMatches.AddRange(bombBlastMatches.Distinct().Except(additionalBombMatches).Except(currentBombBlastMatches));

        additionalBombMatches = additionalBombMatches.Except(currentBombMatches).ToList();
        if (additionalBombMatches.Count > 0)
        {
            for (int i = 0; i < additionalBombMatches.Count; i++)
            {
                currentBombMatches.Add(additionalBombMatches[i]);
            }

            CheckForBombBlasts(additionalBombMatches);
        }
    }

    private IEnumerable<SC_Gem> MarkBombArea(Vector2Int bombPos, int _blastSize)
    {
        List<SC_Gem> bombBlastAreaMatches = new();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int distance = Mathf.Abs(x - bombPos.x) + Mathf.Abs(y - bombPos.y);
                if (distance <= _blastSize)
                {
                    bombBlastAreaMatches.Add(allGems[x, y]);
                }
            }
        }

        return bombBlastAreaMatches.Distinct().Except(currentMatches).Except(currentBombMatches);
    }

    public void CleanBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (allGems[x, y] == null) continue;

                if (allGems[x, y].GetComponent<Poolable>().IsPooled)
                {
                    SetGem(x, y, null);
                }
            }
        }
    }
}

