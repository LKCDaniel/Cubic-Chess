using UnityEngine;
using System.Collections;

public class ChessPiece : MonoBehaviour
{
    public float yOffset = 0, scale = 100, zRotation = 0;
    public int chessX, chessY, chessZ; // X, Y, Z coordinates in the chess board
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

    public void SetPosition(Vector3 newPosition)
    {
        newPosition.y += yOffset; // Adjust the height based on yOffset
        Debug.Log($"{name}'s position set to {newPosition}");
        transform.position = newPosition;
    }

    public void SetHighLight(bool on)
    {
        if (on)
            material.EnableKeyword("_EMISSION");
        else
            material.DisableKeyword("_EMISSION");
    }


    // lerp the chess piece to a new position
    public void LerpToPosition(Vector3 newPosition, float duration)
    {
        StartCoroutine(LerpPosition(newPosition, duration));
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
