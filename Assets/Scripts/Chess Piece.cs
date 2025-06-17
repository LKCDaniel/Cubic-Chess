using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class ChessPiece : MonoBehaviour
{
    public float yOffset = 0, scale = 100, zRotation = 0;
    public int3 chessPosition; // X, Y, Z coordinates in the chess board
    private Material material;

    void OnEnable()
    {
        transform.localScale = new Vector3(scale, scale, scale);
        transform.rotation = Quaternion.Euler(-90, 0, zRotation);
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {

    }

    private Vector3 TargetPosition(int3 p)
    {
        float sep = GameManager.Instance.separation;
        Vector3 position = new Vector3(p.x * sep - 1.5f * sep, p.y * sep - 1.5f * sep, p.z * sep - 1.5f * sep);
        position.y += yOffset; // Adjust the height based on yOffset
        return position;
    }

    public void SetPosition(int3 chessPosition)
    {
        transform.position = TargetPosition(chessPosition);
    }

    public void SetHighLight(bool on)
    {
        if (on)
            material.EnableKeyword("_EMISSION");
        else
            material.DisableKeyword("_EMISSION");
    }

    public void Eaten()
    {

    }

    // lerp the chess piece to a new position
    public void Goto(int3 newPosition, float duration)
    {
        StartCoroutine(LerpPosition(TargetPosition(newPosition), duration));
    }

    private IEnumerator LerpPosition(Vector3 newPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        newPosition.y += yOffset; // Adjust the new position's height
        float elapsedTime = 0;
        Debug.Log($"{name} lerping from {startPosition} to {newPosition} over {duration} seconds");
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, newPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = newPosition; // Ensure final position is set
    }
}
