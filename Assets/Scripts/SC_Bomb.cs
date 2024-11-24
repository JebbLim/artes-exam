using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Bomb : SC_Gem
{
    [Header("Bomb Configuration")]
    public int BlastSize = 1;

    [Header("Bomb References")]
    public SpriteRenderer ColorSprite;

    public override void SetupGem(SC_GameLogic _ScGameLogic, Vector2Int _Position)
    {
        base.SetupGem(_ScGameLogic, _Position);

        ColorSprite.sprite = GameConfig.Config.GetGemPrefab(type).spriteRenderer.sprite;
    }

    public override void Despawn()
    {
        StartCoroutine(DespawnCO());
    }

    private IEnumerator DespawnCO()
    {
        yield return new WaitForSeconds(0.15f);
        Instantiate(destroyEffect, new Vector2(posIndex.x, posIndex.y), Quaternion.identity);
        GetComponent<Poolable>().Pool();
    }
}
