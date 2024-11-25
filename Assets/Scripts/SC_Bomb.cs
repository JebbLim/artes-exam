using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Bomb : SC_Gem
{
    [Header("Bomb Configuration")]
    public int BlastSize = 1;
    public float DespawnDelay = 0.15f;

    [Header("Bomb References")]
    public SpriteRenderer ColorSprite;

    public override void SetupGem(SC_GameLogic _scGameLogic, Vector2Int _position)
    {
        base.SetupGem(_scGameLogic, _position);

        ColorSprite.sprite = GameConfig.Config.GetGemPrefab(Type).SpriteRenderer.sprite;
    }

    public override void Despawn()
    {
        StartCoroutine(DespawnCO());
    }

    private IEnumerator DespawnCO()
    {
        yield return new WaitForSeconds(DespawnDelay);
        Instantiate(DestroyEffect, new Vector2(PosIndex.x, PosIndex.y), Quaternion.identity);
        GetComponent<Poolable>().Pool();
    }
}
