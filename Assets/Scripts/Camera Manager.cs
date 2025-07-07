using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Mathf;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public float minPhi, maxPhi, minRadius, maxRadius;
    public float initRadius;
    public float initTheta, initPhi; // initial horizontal angle 0-360, vertical angle
    public float cameraYOffset; // camera height offset
    [HideInInspector]
    public float radius, theta, phi; // current camera angles
    private float storedRadius, storedTheta, storedPhi; // store white and dark camera angles


    public void EntryCamera(float entryTime, Action onComplete = null)
    {
        ShiftCamera(initRadius, initTheta, initPhi, entryTime, () =>
        {
            storedRadius = initRadius;
            storedTheta = initTheta;
            storedPhi = initPhi;
            onComplete?.Invoke();
        });
    }

    public void UpdateCamera()
    {
        radius = Clamp(radius, minRadius, maxRadius);
        theta %= 360;
        phi = Clamp(phi, minPhi, maxPhi);
        float x = radius * Sin(phi * Deg2Rad) * Cos(theta * Deg2Rad);
        float y = radius * Cos(phi * Deg2Rad);
        float z = radius * Sin(phi * Deg2Rad) * Sin(theta * Deg2Rad);
        Camera.main.transform.position = new Vector3(x, y + cameraYOffset, z);
        Camera.main.transform.LookAt(new Vector3(0, cameraYOffset, 0));
    }

    public void SwitchCamera(float duration) => ShiftCamera(storedRadius, storedTheta, storedPhi, duration);

    private void ShiftCamera(float targetRadius, float targetTheta, float targetPhi, float duration, Action onComplete = null)
    {
        StartCoroutine(MoveCameraCoroutine(targetRadius, targetTheta, targetPhi, duration, onComplete));
        storedRadius = radius;
        storedTheta = theta;
        storedPhi = phi;

        IEnumerator MoveCameraCoroutine(float targetRadius, float targetTheta, float targetPhi, float duration, Action onComplete)
        {
            Vector3 startPosition = Camera.main.transform.position;
            float elapsedTime = 0;
            float beginRadius = radius;
            float beginTheta = theta;
            float beginPhi = phi;
            float deltaTheta = targetTheta - theta;
            if (deltaTheta > 180) deltaTheta -= 360;
            else if (deltaTheta < -180) deltaTheta += 360;

            while (elapsedTime < duration)
            {
                float time = SmoothStep(0, 1, elapsedTime / duration);
                radius = Lerp(beginRadius, targetRadius, time);
                theta = Lerp(0, deltaTheta, time) + beginTheta;
                phi = Lerp(beginPhi, targetPhi, time);
                UpdateCamera();
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            radius = targetRadius;
            theta = targetTheta;
            phi = targetPhi;
            UpdateCamera();
            onComplete?.Invoke();
        }

    }


}
