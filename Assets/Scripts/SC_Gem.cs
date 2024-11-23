using System.Collections;
using UnityEngine;

public class SC_Gem : MonoBehaviour
{
    [Header("Configuration")]
    public GlobalEnums.GemType type;
    public int scoreValue = 10;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public GameObject destroyEffect;

    private SC_GameLogic scGameLogic;
    private GameConfig gameConfig;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private bool mousePressed;
    private float swipeAngle = 0;
    private SC_Gem otherGem;
    private Vector2Int previousPos;

    [HideInInspector] public Vector2Int posIndex;
    [HideInInspector] public bool isMatch;

    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex, gameConfig.GemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            scGameLogic.SetGem(posIndex.x, posIndex.y, this);
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
        isMatch = false;
        firstTouchPosition = Vector2.zero;
        finalTouchPosition = Vector2.zero;

        mousePressed = false;
        swipeAngle = 0;
        otherGem = null;
        previousPos = Vector2Int.zero;

        posIndex = Vector2Int.zero;
    }

    public virtual void SetupGem(SC_GameLogic _ScGameLogic, Vector2Int _Position)
    {
        gameConfig = GameConfig.Config;

        posIndex = _Position;
        scGameLogic = _ScGameLogic;
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
        previousPos = posIndex;

        if (swipeAngle < 45 && swipeAngle > -45 && posIndex.x < gameConfig.RowsSize - 1)
        {
            otherGem = scGameLogic.GetGem(posIndex.x + 1, posIndex.y);
            otherGem.posIndex.x--;
            posIndex.x++;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < gameConfig.ColsSize - 1)
        {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y + 1);
            otherGem.posIndex.y--;
            posIndex.y++;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
        {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y - 1);
            otherGem.posIndex.y++;
            posIndex.y--;
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && posIndex.x > 0)
        {
            otherGem = scGameLogic.GetGem(posIndex.x - 1, posIndex.y);
            otherGem.posIndex.x++;
            posIndex.x--;
        }

        scGameLogic.SetGem(posIndex.x, posIndex.y, this);
        scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

        StartCoroutine(CheckMoveCo());
    }

    public IEnumerator CheckMoveCo()
    {
        scGameLogic.SetState(GlobalEnums.GameState.Wait);

        yield return new WaitForSeconds(.5f);
        scGameLogic.FindAllMatches();

        if (otherGem != null)
        {
            if (isMatch == false && otherGem.isMatch == false)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                scGameLogic.SetGem(posIndex.x, posIndex.y, this);
                scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

                yield return new WaitForSeconds(.5f);
                scGameLogic.SetState(GlobalEnums.GameState.Move);
            }
            else
            {
                scGameLogic.UserLastInputData(posIndex, type);
                scGameLogic.DestroyMatches();
            }
        }
    }
}
