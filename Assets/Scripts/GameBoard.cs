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

    public bool MatchesAt(Vector2Int _positionToCheck, SC_Gem _gemToCheck)
    {
        if (_positionToCheck.x > 1)
        {
            if (allGems[_positionToCheck.x - 1, _positionToCheck.y].type == _gemToCheck.type &&
                allGems[_positionToCheck.x - 2, _positionToCheck.y].type == _gemToCheck.type)
                return true;
        }

        if (_positionToCheck.y > 1)
        {
            if (allGems[_positionToCheck.x, _positionToCheck.y - 1].type == _gemToCheck.type &&
                allGems[_positionToCheck.x, _positionToCheck.y - 2].type == _gemToCheck.type)
                return true;
        }

        return false;
    }

    public void SetGem(int _x, int _y, SC_Gem _gem)
    {
        allGems[_x, _y] = _gem;
    }

    public SC_Gem GetGem(int _x, int _y)
    {
        return allGems[_x, _y];
    }

    public bool IsTypeMatch(int _x, int _y, GlobalEnums.GemType _gemType)
    {
        if (_x < 0 || _y < 0) return false;
        if (_x > Width - 1 || _y > Height - 1) return false;

        if (_x >= 0 && _x <= Width - 1)
        {
            SC_Gem leftGem = (_x > 0) ? allGems[_x - 1, _y] : null;
            SC_Gem rightGem = (_x < Width - 1) ? allGems[_x + 1, _y] : null;

            // Check for empty spots
            if (leftGem != null && rightGem != null)
            {
                //Match
                if (leftGem.type == _gemType && rightGem.type == _gemType)
                {
                    return true;
                }
            }
            // Check for extended matching
            else
            {
                if (_x > 1)
                {
                    SC_Gem leftLeftGem = allGems[_x - 2, _y];
                    if (leftGem != null && leftLeftGem != null)
                    {
                        // Match
                        if (leftGem.type == _gemType && leftLeftGem.type == _gemType)
                        {
                            return true;
                        }
                    }
                }

                if (_x < Width - 2)
                {
                    SC_Gem rightRightGem = allGems[_x + 2, _y];
                    if (rightGem != null && rightRightGem != null)
                    {
                        // Match
                        if (rightGem.type == _gemType && rightRightGem.type == _gemType)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        if (_y >= 0 && _y <= Height - 1)
        {
            SC_Gem aboveGem = (_y > 0) ? allGems[_x, _y - 1] : null;
            SC_Gem belowGem = (_y < Height - 1) ? allGems[_x, _y + 1] : null;

            // Check for empty spots
            if (aboveGem != null && belowGem != null)
            {
                //Match
                if (aboveGem.type == _gemType && belowGem.type == _gemType)
                {
                    return true;
                }
            }
            // Check for extended matching
            else
            {
                if (_y > 1)
                {
                    SC_Gem aboveAboveGem = allGems[_x, _y - 2];
                    if (aboveGem != null && aboveAboveGem != null)
                    {
                        // Match
                        if (aboveGem.type == _gemType && aboveAboveGem.type == _gemType)
                        {
                            return true;
                        }
                    }
                }

                if (_y < Height - 2)
                {
                    SC_Gem belowBelowGem = allGems[_x, _y + 2];
                    if (belowGem != null && belowBelowGem != null)
                    {
                        // Match
                        if (belowGem.type == _gemType && belowBelowGem.type == _gemType)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
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
                            if (leftGem.type == currentGem.type && rightGem.type == currentGem.type)
                            {
                                currentGem.isMatch = true;
                                leftGem.isMatch = true;
                                rightGem.isMatch = true;
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
                            if (aboveGem.type == currentGem.type && belowGem.type == currentGem.type)
                            {
                                currentGem.isMatch = true;
                                aboveGem.isMatch = true;
                                belowGem.isMatch = true;
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
                if (currentMatchesComboCounter.ContainsKey(currentMatches[i].type) == false)
                {
                    currentMatchesComboCounter.Add(currentMatches[i].type, 1);
                }
                else
                {
                    currentMatchesComboCounter[currentMatches[i].type]++;
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

            IEnumerable<SC_Gem> markArea = MarkBombArea(bomb.posIndex, bomb.BlastSize);
            bombBlastMatches.AddRange(markArea);

            foreach (SC_Gem gem in bombBlastMatches)
            {
                if (gem is SC_Bomb)
                {
                    additionalBombMatches.Add(gem as SC_Bomb);
                }
            }
        }

        currentBombBlastMatches.AddRange(bombBlastMatches.Distinct());
        //currentBombBlastMatches = currentBombBlastMatches.Distinct().ToList();

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

