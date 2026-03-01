using UnityEngine;

public class LF : MonoBehaviour
{
    public SpriteRenderer targetL;
    public Sprite[] texturesL;
    public int idxL;

    public void UpdateL(int x)
    {
        targetL.sprite = texturesL[x];
    }
}
