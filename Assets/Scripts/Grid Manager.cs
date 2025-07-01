using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public GameObject darkGridPrefab, whiteGridPrefab;
    private List<GameObject> grids = new List<GameObject>();

    void Start()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    float sep = GameManager.Instance.separation;
                    Vector3 position = new Vector3(x * sep - 1.5f * sep, y * sep - 2 * sep, z * sep - 1.5f * sep);
                    GameObject prefab;
                    prefab = ((x + y + z) % 2 == 0) ? whiteGridPrefab : darkGridPrefab;
                    GameObject grid = Instantiate(prefab, position, Quaternion.identity);
                    grid.transform.localScale = 0.1f * sep * Vector3.one;
                    grid.GetComponent<MoveableObject>().yOffset = -0.5f * sep;
                    grids.Add(grid);
                }
            }
        }
    }

    public void Revolve()
    {
        foreach (var grid in grids)
        {
            grid.GetComponent<MoveableObject>().RevolveAlongAxisZ();
        }
    }


}
