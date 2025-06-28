using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class CubeManager : MonoBehaviour
{
    public static CubeManager Instance;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public GameObject redCubePrefab, greenCubePrefab, orangeCubePrefab;
    private List<GameObject> cubes = new List<GameObject>();
    private GameObject warningCube;

    public void SetCubes(List<int3> greenCubes, List<int3> redCubes)
    {
        ClearMoveableCubes();
        foreach (var pos in redCubes)
            CreateCube(pos.x, pos.y, pos.z, true);

        foreach (var pos in greenCubes)
            CreateCube(pos.x, pos.y, pos.z, false);
    }

    public void SetWarningCube(int3 pos)
    {
        CreateCube(pos.x, pos.y, pos.z, false, true);
    }

    public void ClearMoveableCubes()
    {
        foreach (var cube in cubes)
            Destroy(cube);
        cubes.Clear();
    }

    public void ClearWarningCube()
    {
        if (warningCube == null)
            return;
        Destroy(warningCube);
        warningCube = null;
    }

    private void CreateCube(int x, int y, int z, bool isRed, bool isOrange = false)

    {
        float sep = GameManager.Instance.separation;
        Vector3 p = new Vector3(x * sep - 1.5f * sep, y * sep - 1.5f * sep, z * sep - 1.5f * sep);
        if (!GameManager.Instance.isWhiteTurn)
            p = new Vector3(-p.x, -p.y, p.z);
        GameObject cubePrefab = isOrange ? orangeCubePrefab : (isRed ? redCubePrefab : greenCubePrefab);
        GameObject cube = Instantiate(cubePrefab, p, Quaternion.identity);
        cube.GetComponent<Cube>().chessPosition = new int3(x, y, z);
        cube.GetComponent<Cube>().isRed = isRed;
        if (isOrange)
        {
            warningCube = cube;
            cube.GetComponent<Cube>().SetEnlargeCube(true);
        }
        else
            cubes.Add(cube);
    }

}

