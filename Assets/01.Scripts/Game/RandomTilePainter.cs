using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomTilePainter : MonoBehaviour
{
    public Tilemap tilemap;               // 타일맵 레퍼런스
    public TileBase[] wallTiles;          // 벽 타일 5종류

    void Start()
    {
        tilemap = GetComponent<Tilemap>();

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase currentTile = tilemap.GetTile(pos);

                if (currentTile != null)
                {
                    // 기존 타일이 있는 경우만 랜덤 교체
                    TileBase randomTile = wallTiles[Random.Range(0, wallTiles.Length)];
                    tilemap.SetTile(pos, randomTile);
                }
            }
        }
    }
}