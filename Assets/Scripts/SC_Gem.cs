using System.Collections;
using UnityEngine;

public class SC_Gem : MonoBehaviour
{
    [Header("Configuration")]
    public GlobalEnums.GemType Type;
    public int ScoreValue = 10;

    [Header("References")]
    public SpriteRenderer SpriteRenderer;
    public GameObject DestroyEffect;

    private SC_GameLogic scGameLogic;
    private GameConfig gameConfig;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private bool mousePressed;
    private float swipeAngle = 0;
    private SC_Gem otherGem;
    private Vector2Int previousPos;

    [HideInInspector] public Vector2Int PosIndex;
    public bool IsMatch { get; set; }

    private void Update()
    {
        if (Vector2.Distance(transform.position, PosIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, PosIndex, gameConfig.GemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(PosIndex.x, PosIndex.y, 0);
            scGameLogic.SetGem(PosIndex.x, PosIndex.y, this);
        }
        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            if (scGameLogic.CurrentState == GlobalEnums.GameState.Move)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    private void OnDisable()
    {
        IsMatch = false;
        firstTouchPosition = Vector2.zero;
        finalTouchPosition = Vector2.zero;

        mousePressed = false;
        swipeAngle = 0;
        otherGem = null;
        previousPos = Vector2Int.zero;

        PosIndex = Vector2Int.zero;

        StopAllCoroutines();
    }

    public virtual void SetupGem(SC_GameLogic _scGameLogic, Vector2Int _position)
    {
        gameConfig = GameConfig.Config;

        PosIndex = _position;
        scGameLogic = _scGameLogic;
    }

    private void OnMouseDown()
    {
        if (scGameLogic.CurrentState == GlobalEnums.GameState.Move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;

        if (Vector3.Distance(firstTouchPosition, finalTouchPosition) > .5f)
            MovePieces();
    }

    private void MovePieces()
    {
        previousPos = PosIndex;

        if (swipeAngle < 45 && swipeAngle > -45 && PosIndex.x < gameConfig.RowsSize - 1)
        {
            otherGem = scGameLogic.GetGem(PosIndex.x + 1, PosIndex.y);
            otherGem.PosIndex.x--;
            PosIndex.x++;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && PosIndex.y < gameConfig.ColsSize - 1)
        {
            otherGem = scGameLogic.GetGem(PosIndex.x, PosIndex.y + 1);
            otherGem.PosIndex.y--;
            PosIndex.y++;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && PosIndex.y > 0)
        {
            otherGem = scGameLogic.GetGem(PosIndex.x, PosIndex.y - 1);
            otherGem.PosIndex.y++;
            PosIndex.y--;
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && PosIndex.x > 0)
        {
            otherGem = scGameLogic.GetGem(PosIndex.x - 1, PosIndex.y);
            otherGem.PosIndex.x++;
            PosIndex.x--;
        }

        scGameLogic.SetGem(PosIndex.x, PosIndex.y, this);
        scGameLogic.SetGem(otherGem.PosIndex.x, otherGem.PosIndex.y, otherGem);

        StartCoroutine(CheckMoveCO());
    }

    public IEnumerator CheckMoveCO()
    {
        scGameLogic.SetState(GlobalEnums.GameState.Wait);

        yield return new WaitForSeconds(.5f);
        scGameLogic.FindAllMatches();

        if (otherGem != null)
        {
            if (IsMatch == false && otherGem.IsMatch == false)
            {
                otherGem.PosIndex = PosIndex;
                PosIndex = previousPos;

                scGameLogic.SetGem(PosIndex.x, PosIndex.y, this);
                scGameLogic.SetGem(otherGem.PosIndex.x, otherGem.PosIndex.y, otherGem);

                yield return new WaitForSeconds(.5f);
                scGameLogic.SetState(GlobalEnums.GameState.Move);
            }
            else
            {
                scGameLogic.UserLastInputData(new SC_GameLogic.UserLastMovedData(PosIndex, Type),
                                              new SC_GameLogic.UserLastMovedData(otherGem.PosIndex, otherGem.Type));
                scGameLogic.DestroyMatches();
            }
        }
    }

    public virtual void Despawn()
    {
        Instantiate(DestroyEffect, new Vector2(PosIndex.x, PosIndex.y), Quaternion.identity);
        GetComponent<Poolable>().Pool();
    }
}
