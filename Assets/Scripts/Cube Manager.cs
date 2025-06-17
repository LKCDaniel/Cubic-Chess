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
    private GameObject[,,] chessCubes = new GameObject[4, 4, 4];

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
        foreach (var cube in chessCubes)
        {
            if (cube != null) Destroy(cube);
        }
        chessCubes = new GameObject[4, 4, 4];
    }

    private void CreateCube(int x, int y, int z, bool isRed)
    {
        float sep = GameManager.Instance.separation;
        GameObject cubePrefab = isRed ? redCubePrefab : greenCubePrefab;
        Vector3 p = new Vector3(x * sep - 1.5f * sep, y * sep - 1.5f * sep, z * sep - 1.5f * sep);
        GameObject cube = Instantiate(cubePrefab, p, Quaternion.identity);
        cube.transform.localScale = new Vector3(sep, sep, sep);
        chessCubes[x, y, z] = cube;
    }

}
