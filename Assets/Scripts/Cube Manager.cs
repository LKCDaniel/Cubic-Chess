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
        Instance = this;
    }

    public GameObject redCubePrefab, greenCubePrefab;
    private List<GameObject> cubes = new List<GameObject>();

    public void SetCubes(List<int3> greenCubes, List<int3> redCubes)
    {
        ClearCubes();
        foreach (var pos in redCubes)
            CreateCube(pos.x, pos.y, pos.z, true);

        foreach (var pos in greenCubes)
            CreateCube(pos.x, pos.y, pos.z, false);
    }

    public void ClearCubes()
    {
        foreach (var cube in cubes)
            Destroy(cube);
        cubes.Clear();
    }

    private void CreateCube(int x, int y, int z, bool isRed)
    {
        float sep = GameManager.Instance.separation;
        Vector3 p = new Vector3(x * sep - 1.5f * sep, y * sep - 1.5f * sep, z * sep - 1.5f * sep);
        GameObject cube = Instantiate(isRed ? redCubePrefab : greenCubePrefab, p, Quaternion.identity);
        cube.GetComponent<Cube>().chessPosition = new int3(x, y, z);
        cube.GetComponent<Cube>().isRed = isRed;
        cubes.Add(cube);
    }

}

