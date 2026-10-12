using UnityEngine;

namespace FoodSurvivors.Core
{
    public class MapBoundsManager : MonoBehaviour
    {
        public static MapBoundsManager Instance { get; private set; }

        [Header("Map Area Settings")]
        [Tooltip("Размеры всей площади карты по X и Z")]
        public Vector2 mapSize = new Vector2(160f, 160f);
        public Vector3 mapCenter = Vector3.zero;

        [Header("Chunk Grid Settings")]
        [Tooltip("На сколько чанков делить карту по оси X и Z (например, 5x5 чанков)")]
        [Range(2, 16)]
        public int chunksCountX = 5;
        [Range(2, 16)]
        public int chunksCountZ = 5;

        [Header("Pac-Man Boundary Teleport")]
        public bool enablePacmanTeleport = true;

        public float MinX => mapCenter.x - mapSize.x / 2f;
        public float MaxX => mapCenter.x + mapSize.x / 2f;
        public float MinZ => mapCenter.z - mapSize.y / 2f;
        public float MaxZ => mapCenter.z + mapSize.y / 2f;

        public float ChunkSizeX => mapSize.x / chunksCountX;
        public float ChunkSizeZ => mapSize.y / chunksCountZ;

        [Header("Blacklisted Chunks (Запрещенные зоны)")]
        [Tooltip("Координаты чанков [X, Z], где спавн мобов и босса запрещен")]
        public System.Collections.Generic.List<Vector2Int> blacklistedChunks = new System.Collections.Generic.List<Vector2Int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Если скрипт висит на объекте с BoxCollider (твоем кубе), берем размеры прямо из него!
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                mapCenter = transform.position + box.center;
                mapSize = new Vector2(box.size.x * transform.lossyScale.x, box.size.z * transform.lossyScale.z);
            }
        }

        private void LateUpdate()
        {
            if (!enablePacmanTeleport) return;

            // Проверяем и телепортируем игрока на противоположный край
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Vector3 pos = playerObj.transform.position;
                if (CheckAndWrapPosition(ref pos))
                {
                    playerObj.transform.position = pos;
                    Rigidbody rb = playerObj.GetComponent<Rigidbody>();
                    if (rb != null) rb.position = pos;

                    // Уведомляем камеру, чтобы она не делала резкий рывок через всю карту
                    CameraFollow cam = Object.FindFirstObjectByType<CameraFollow>();
                    if (cam != null) cam.transform.position = pos + cam.offset;
                }
            }
        }

        /// <summary>
        /// Телепортирует координату на противоположный край при выходе за рамку
        /// </summary>
        public bool CheckAndWrapPosition(ref Vector3 pos)
        {
            bool wrapped = false;

            if (pos.x > MaxX) { pos.x = MinX + 1.5f; wrapped = true; }
            else if (pos.x < MinX) { pos.x = MaxX - 1.5f; wrapped = true; }

            if (pos.z > MaxZ) { pos.z = MinZ + 1.5f; wrapped = true; }
            else if (pos.z < MinZ) { pos.z = MaxZ - 1.5f; wrapped = true; }

            return wrapped;
        }

        /// <summary>
        /// Возвращает координаты текущего чанка по мировой позиции (например, чанк [2, 3])
        /// </summary>
        public Vector2Int GetChunkCoords(Vector3 worldPos)
        {
            int cx = Mathf.FloorToInt((worldPos.x - MinX) / ChunkSizeX);
            int cz = Mathf.FloorToInt((worldPos.z - MinZ) / ChunkSizeZ);
            return new Vector2Int(Mathf.Clamp(cx, 0, chunksCountX - 1), Mathf.Clamp(cz, 0, chunksCountZ - 1));
        }

        /// <summary>
        /// Возвращает случайную мировую позицию внутри соседнего чанка от игрока
        /// </summary>
        public Vector3 GetRandomPositionNearPlayerChunk(Vector3 playerPos)
        {
            Vector2Int playerChunk = GetChunkCoords(playerPos);

            int targetChunkX = playerChunk.x;
            int targetChunkZ = playerChunk.y;

            // Ищем разрешенный соседний чанк (до 10 попыток)
            for (int attempts = 0; attempts < 10; attempts++)
            {
                targetChunkX = Mathf.Clamp(playerChunk.x + Random.Range(-1, 2), 0, chunksCountX - 1);
                targetChunkZ = Mathf.Clamp(playerChunk.y + Random.Range(-1, 2), 0, chunksCountZ - 1);

                Vector2Int cand = new Vector2Int(targetChunkX, targetChunkZ);
                if (!blacklistedChunks.Contains(cand))
                {
                    break; // Найден чистый чанк!
                }
            }

            float chunkMinX = MinX + targetChunkX * ChunkSizeX;
            float chunkMinZ = MinZ + targetChunkZ * ChunkSizeZ;

            float randX = Random.Range(chunkMinX + 2f, chunkMinX + ChunkSizeX - 2f);
            float randZ = Random.Range(chunkMinZ + 2f, chunkMinZ + ChunkSizeZ - 2f);

            return new Vector3(randX, playerPos.y, randZ);
        }

        // Отрисовка сетки чанков в окне Scene
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(new Vector3(mapCenter.x, 1f, mapCenter.z), new Vector3(mapSize.x, 2f, mapSize.y));

            Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
            for (int x = 0; x <= chunksCountX; x++)
            {
                float posX = MinX + x * ChunkSizeX;
                Gizmos.DrawLine(new Vector3(posX, 1f, MinZ), new Vector3(posX, 1f, MaxZ));
            }
            for (int z = 0; z <= chunksCountZ; z++)
            {
                float posZ = MinZ + z * ChunkSizeZ;
                Gizmos.DrawLine(new Vector3(MinX, 1f, posZ), new Vector3(MaxX, 1f, posZ));
            }

            Gizmos.color = Color.red;
            foreach (var b in blacklistedChunks)
            {
                float cx = MinX + b.x * ChunkSizeX + ChunkSizeX / 2f;
                float cz = MinZ + b.y * ChunkSizeZ + ChunkSizeZ / 2f;
                Gizmos.DrawWireCube(new Vector3(cx, 1f, cz), new Vector3(ChunkSizeX * 0.9f, 2f, ChunkSizeZ * 0.9f));
            }
        }
    }
}