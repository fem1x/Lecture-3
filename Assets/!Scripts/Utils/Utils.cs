using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Utils
{
    public static float NumberInRange(float baseValue, float deltaValue)
    {
        return UnityEngine.Random.Range(baseValue - deltaValue, baseValue + deltaValue);
    }

    public static bool TryRandomChance(float chance)
    {
        float randomProc = Random.Range(0f, 100f);
        return (randomProc <= chance);
    }

    public static List<T> ShuffledList<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public static List<Vector3Int> GetTilesList(Tilemap tilemap)
    {
        List<Vector3Int> tilesList = new();
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                tilesList.Add(pos);
            }
        }
        return tilesList;
    }

    /// <summary>
    /// Checks if the layerMask contains layer
    /// </summary>
    public static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
