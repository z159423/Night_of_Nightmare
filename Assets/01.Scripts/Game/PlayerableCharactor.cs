using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerableCharactor : Charactor
{
    public int gold = 0;
    public int energy = 0;

    public Bed currentFindBed;
    //현재 있는 방
    public Room currentActiveRoom;

    public Define.CharactorType charactorType;

    public bool die = false;

    public IEnumerator GetGold()
    {
        yield return new WaitForSeconds(1f);

        gold += 1;
    }

    public override void SetBodySkin()
    {
        if (bodySpriteRenderer == null)
        {
            bodySpriteRenderer = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
        }
        bodySpriteRenderer.sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bed>(out Bed bed))
        {
            if (!bed.active)
            {
                currentActiveRoom = bed.OnActive(this);
                gameObject.SetActive(false);
            }
        }
    }

    public abstract void Die();
}
