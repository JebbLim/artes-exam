using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard
{
    #region Variables

    private int height = 0;
    public int Height { get { return height; } }

    private int width = 0;
    public int Width { get { return width; } }

    private SC_Gem[,] allGems;
    //  public Gem[,] AllGems { get { return allGems; } }

    public int Score { get; set; }

    private List<SC_Gem> currentMatches = new();
    public IReadOnlyList<SC_Gem> CurrentMatches => currentMatches;
    #endregion

    public GameBoard(int _Width, int _Height)
    {
        height = _Height;
        width = _Width;
        allGems = new SC_Gem[width, height];
    }
    public bool MatchesAt(Vector2Int _PositionToCheck, SC_Gem _GemToCheck)
    {
        if (_PositionToCheck.x > 1)
        {
            if (allGems[_PositionToCheck.x - 1, _PositionToCheck.y].type == _GemToCheck.type &&
                allGems[_PositionToCheck.x - 2, _PositionToCheck.y].type == _GemToCheck.type)
                return true;
        }

        if (_PositionToCheck.y > 1)
        {
            if (allGems[_PositionToCheck.x, _PositionToCheck.y - 1].type == _GemToCheck.type &&
                allGems[_PositionToCheck.x, _PositionToCheck.y - 2].type == _GemToCheck.type)
                return true;
        }

        return false;
    }

    public void SetGem(int _X, int _Y, SC_Gem _Gem)
    {
        allGems[_X, _Y] = _Gem;
    }
    public SC_Gem GetGem(int _X, int _Y)
    {
        return allGems[_X, _Y];
    }

    public bool IsMatch(int x, int y, GlobalEnums.GemType gemType)
    {
        if (x < 0 || y < 0) return false;
        if (x > width - 1 || y > height - 1) return false;

        Debug.LogWarning($"Checking: {gemType}");

        if (x > 0 && x < width - 1)
        {
            SC_Gem leftGem = allGems[x - 1, y];
            SC_Gem rightGem = allGems[x + 1, y];

            // Check for empty spots
            if (leftGem != null && rightGem != null)
            {
                //Match
                if (leftGem.type == gemType && rightGem.type == gemType)
                {
                    Debug.LogWarning($"Check Confirmed [Horizontal]: {gemType}");
                    return true;
                }
            }
            // Check for extended matching
            else
            {
                if (x > 1)
                {
                    SC_Gem leftLeftGem = allGems[x - 2, y];
                    if (leftGem != null && leftLeftGem != null)
                    {
                        // Match
                        if (leftGem.type == gemType && leftLeftGem.type == gemType)
                        {
                            Debug.LogWarning($"Extended Check Confirmed [Left-Horizontal] : {gemType}");
                            return true;
                        }
                    }
                }

                if (x < width - 2)
                {
                    SC_Gem rightRightGem = allGems[x + 2, y];
                    if (rightGem != null && rightRightGem != null)
                    {
                        // Match
                        if (rightGem.type == gemType && rightRightGem.type == gemType)
                        {
                            Debug.LogWarning($"Extended Check Confirmed [Right-Horizontal] : {gemType}");
                            return true;
                        }
                    }
                }
            }
        }

        if (y > 0 && y < height - 1)
        {
            SC_Gem aboveGem = allGems[x, y - 1];
            SC_Gem belowGem = allGems[x, y + 1];

            // Check for empty spots
            if (aboveGem != null && belowGem != null)
            {
                //Match
                if (aboveGem.type == gemType && belowGem.type == gemType)
                {
                    Debug.LogWarning($"Check Confirmed [Vertical]: {gemType}");
                    return true;
                }
            }
            // Check for extended matching
            else
            {
                if (y > 1)
                {
                    SC_Gem aboveAboveGem = allGems[x, y - 2];
                    if (aboveGem != null && aboveAboveGem != null)
                    {
                        // Match
                        if (aboveGem.type == gemType && aboveAboveGem.type == gemType)
                        {
                            Debug.LogWarning($"Extended Check Confirmed [Above-Vertical] : {gemType}");
                            return true;
                        }
                    }
                }

                if (y < height - 2)
                {
                    SC_Gem belowBelowGem = allGems[x, y + 2];
                    if (belowGem != null && belowBelowGem != null)
                    {
                        // Match
                        if (belowGem.type == gemType && belowBelowGem.type == gemType)
                        {
                            Debug.LogWarning($"Extended Check Confirmed [Below-Vertical] : {gemType}");
                            return true;
                        }
                    }
                }
            }
        }

        Debug.Log($"Clear: {gemType}");
        return false;
    }

    public void FindAllMatches()
    {
        currentMatches.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                SC_Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    if (x > 0 && x < width - 1)
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

                    if (y > 0 && y < height - 1)
                    {
                        SC_Gem aboveGem = allGems[x, y - 1];
                        SC_Gem bellowGem = allGems[x, y + 1];
                        //checking no empty spots
                        if (aboveGem != null && bellowGem != null)
                        {
                            //Match
                            if (aboveGem.type == currentGem.type && bellowGem.type == currentGem.type)
                            {
                                currentGem.isMatch = true;
                                aboveGem.isMatch = true;
                                bellowGem.isMatch = true;
                                currentMatches.Add(currentGem);
                                currentMatches.Add(aboveGem);
                                currentMatches.Add(bellowGem);
                            }
                        }
                    }
                }
            }

        if (currentMatches.Count > 0)
            currentMatches = currentMatches.Distinct().ToList();

        CheckForBombs();
    }

    public void CheckForBombs()
    {
        for (int i = 0; i < currentMatches.Count; i++)
        {
            SC_Gem gem = currentMatches[i];
            int x = gem.posIndex.x;
            int y = gem.posIndex.y;

            if (gem.posIndex.x > 0)
            {
                if (allGems[x - 1, y] != null && allGems[x - 1, y].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x - 1, y), allGems[x - 1, y].blastSize);
            }

            if (gem.posIndex.x + 1 < width)
            {
                if (allGems[x + 1, y] != null && allGems[x + 1, y].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x + 1, y), allGems[x + 1, y].blastSize);
            }

            if (gem.posIndex.y > 0)
            {
                if (allGems[x, y - 1] != null && allGems[x, y - 1].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x, y - 1), allGems[x, y - 1].blastSize);
            }

            if (gem.posIndex.y + 1 < height)
            {
                if (allGems[x, y + 1] != null && allGems[x, y + 1].type == GlobalEnums.GemType.Bomb)
                    MarkBombArea(new Vector2Int(x, y + 1), allGems[x, y + 1].blastSize);
            }
        }
    }

    public void MarkBombArea(Vector2Int bombPos, int _BlastSize)
    {
        string _print = "";
        for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++)
        {
            for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (allGems[x, y] != null)
                    {
                        _print += "(" + x + "," + y + ")" + System.Environment.NewLine;
                        allGems[x, y].isMatch = true;
                        currentMatches.Add(allGems[x, y]);
                    }
                }
            }
        }
        currentMatches = currentMatches.Distinct().ToList();
    }
}

