using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using static UnityEngine.Mathf;

public class ChessPiece : MonoBehaviour
{
    public float yOffset, scale;
    public int3 chessPosition; // X, Y, Z coordinates in the chess board
    private Material material;
    public bool isDark2White; // if it is moving from Dark to White
    private bool upsideDown; // if the piece is upside down

    void OnEnable()
    {
        transform.localScale = new Vector3(scale, scale, scale);
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        material = GetComponent<Renderer>().material;
        material.SetInt("_ZWrite", 1); // force depth write, avoid abnormal rendering of transparent objects
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

    private Vector3 TargetPosition(int3 p)
    {
        float sep = GameManager.Instance.separation;
        Vector3 position = new Vector3(p.x * sep - 1.5f * sep, p.y * sep - 1.5f * sep, p.z * sep - 1.5f * sep);
        if (!GameManager.Instance.isWhiteTurn)
            position = new Vector3(-position.x, -position.y, position.z);
        position.y += yOffset; // Adjust the height based on yOffset
        return position;
    }

    // ------------------------------ Animations --------------------------------------------------------------

    public void MoveTo(int3 newPosition, System.Action onComplete = null)
    {
        chessPosition = newPosition;
        StartCoroutine(MoveCoroutine(TargetPosition(newPosition), GameManager.Instance.pieceMoveTime, onComplete));

        IEnumerator MoveCoroutine(Vector3 targetPosition, float duration, System.Action onComplete)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                float t = SmoothStep(0, 1, elapsedTime / duration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;
            onComplete?.Invoke();
        }
    }

    public void Eaten(int3 targetPosition)
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(FadeCoroutine(GameManager.Instance.pieceMoveTime, () =>
        {
            transform.position = TargetPosition(targetPosition);
            StartCoroutine(EmergeCoroutine(GameManager.Instance.pieceMoveTime));
        }));

        // Fade to transparent
        IEnumerator FadeCoroutine(float duration, System.Action onComplete = null)
        {
            float elapsedTime = 0;
            Color initialColor = material.color;
            Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

            while (elapsedTime < duration)
            {
                material.color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            material.color = targetColor;
            onComplete?.Invoke();
        }

        // Fade to opaque
        IEnumerator EmergeCoroutine(float duration)
        {
            float elapsedTime = 0;
            Color initialColor = material.color;
            Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1);

            while (elapsedTime < duration)
            {
                material.color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            material.color = targetColor;
        }
    }

    public void UpsideDown()
    {
        if (upsideDown)
            StartCoroutine(UpsideDownCoroutine(-90, GameManager.Instance.pieceMoveTime));
        else
            StartCoroutine(UpsideDownCoroutine(90, GameManager.Instance.pieceMoveTime));
        upsideDown = !upsideDown;

        IEnumerator UpsideDownCoroutine(float targetAngle, float duration)
        {
            Quaternion startRotation = Quaternion.Euler(-targetAngle, 0, 0);
            Quaternion targetRotation = Quaternion.Euler(targetAngle, 0, 0);
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                float t = SmoothStep(0, 1, elapsedTime / duration);
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.rotation = targetRotation;
        }
    }

    public void RevolveAlongAxisZ() // Revolve around the Z-axis
    {
        StartCoroutine(RotateCoroutine(GameManager.Instance.cameraMoveTime));

        IEnumerator RotateCoroutine(float duration)
        {
            float elapsedTime = 0;
            float x = transform.position.x;
            float y = transform.position.y - yOffset;

            while (elapsedTime < duration)
            {
                float angle = SmoothStep(0, 1, elapsedTime / duration) * 180 * Deg2Rad;
                float outX = x * Cos(angle) - y * Sin(angle);
                float outY = x * Sin(angle) + y * Cos(angle);
                transform.position = new Vector3(outX, outY + yOffset, transform.position.z);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = new Vector3(-x, -y + yOffset, transform.position.z);
        }
    }



}
