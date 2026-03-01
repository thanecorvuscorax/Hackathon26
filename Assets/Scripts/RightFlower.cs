using UnityEngine;

public class RF : MonoBehaviour
{
    public SpriteRenderer targetR;
    public Sprite[] texturesR;
    public int idxR;

    public void UpdateR(int x)
    {
        targetR.sprite = texturesR[x];
    }
}
