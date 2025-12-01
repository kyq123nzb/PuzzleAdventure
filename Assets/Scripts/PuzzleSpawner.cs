using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleSpawnPoint
    {
        public Vector3 position;
        public int puzzleId;
        public GameObject puzzlePrefab;
    }
    
    [Header("拼图生成设置")]
    public List<PuzzleSpawnPoint> spawnPoints = new List<PuzzleSpawnPoint>();
    public bool spawnOnStart = true;
    
    [Header("随机生成设置")]
    public bool useRandomSpawning = false;
    public int minPuzzles = 5;
    public int maxPuzzles = 9;
    public GameObject[] puzzlePrefabs;
    
    private List<GameObject> spawnedPuzzles = new List<GameObject>();
    
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnPuzzles();
        }
    }
    
    public void SpawnPuzzles()
    {
        // 清除已存在的拼图
        ClearSpawnedPuzzles();
        
        if (useRandomSpawning)
        {
            SpawnRandomPuzzles();
        }
        else
        {
            SpawnFixedPuzzles();
        }
    }
    
    void SpawnFixedPuzzles()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.puzzlePrefab != null)
            {
                GameObject puzzle = Instantiate(
                    spawnPoint.puzzlePrefab,
                    spawnPoint.position,
                    Quaternion.identity
                );
                
                // 设置拼图ID
                PuzzleItem puzzleItem = puzzle.GetComponent<PuzzleItem>();
                if (puzzleItem != null)
                {
                    puzzleItem.puzzleId = spawnPoint.puzzleId;
                }
                
                spawnedPuzzles.Add(puzzle);
            }
        }
    }
    
    void SpawnRandomPuzzles()
    {
        if (puzzlePrefabs.Length == 0) return;
        
        int puzzleCount = Random.Range(minPuzzles, maxPuzzles + 1);
        List<int> usedIds = new List<int>();
        
        for (int i = 0; i < puzzleCount; i++)
        {
            // 随机选择拼图预制体
            GameObject prefab = puzzlePrefabs[Random.Range(0, puzzlePrefabs.Length)];
            
            // 生成随机位置（可以改进为预设点）
            Vector3 randomPosition = GetRandomSpawnPosition();
            
            GameObject puzzle = Instantiate(prefab, randomPosition, Quaternion.identity);
            
            // 设置唯一的拼图ID
            PuzzleItem puzzleItem = puzzle.GetComponent<PuzzleItem>();
            if (puzzleItem != null)
            {
                int puzzleId = GetUniquePuzzleId(usedIds);
                puzzleItem.puzzleId = puzzleId;
                usedIds.Add(puzzleId);
            }
            
            spawnedPuzzles.Add(puzzle);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // 简单的随机位置生成，可以根据你的场景改进
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 1f, z);
    }
    
    int GetUniquePuzzleId(List<int> usedIds)
    {
        int id = 1;
        while (usedIds.Contains(id))
        {
            id++;
        }
        return id;
    }
    
    void ClearSpawnedPuzzles()
    {
        foreach (GameObject puzzle in spawnedPuzzles)
        {
            if (puzzle != null)
            {
                Destroy(puzzle);
            }
        }
        spawnedPuzzles.Clear();
    }
    
    // 重置所有拼图（用于游戏重开）
    public void ResetAllPuzzles()
    {
        ClearSpawnedPuzzles();
        SpawnPuzzles();
    }
}