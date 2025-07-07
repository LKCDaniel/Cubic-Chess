using Unity.Mathematics;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private Vector3 smallSize, largeSize, colliderSize;
    public int3 chessPosition; // X, Y, Z coordinates in the chess board
    public bool isRed;

    void Start()
    {
        smallSize = 0.8f * BoardManager.Instance.separation * Vector3.one;
        largeSize = 0.9f * BoardManager.Instance.separation * Vector3.one;
        transform.localScale = smallSize;
        colliderSize = GetComponent<BoxCollider>().size;
    }

    public void SetEnlargeCube(bool big)
    {
        transform.localScale = big ? largeSize : smallSize;
        GetComponent<BoxCollider>().size = colliderSize;
    }
}
