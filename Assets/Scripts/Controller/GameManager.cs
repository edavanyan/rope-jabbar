using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    
    public Node hangingItem;
    public Rope ropePrefab;
    public LevelData levelData;

    private void Start()
    {
        Instance = this;
        List<int> areaIndices = new List<int> { 0, 1, 2, 3 };
        List<int> lengthIndices = new List<int> { 0, 1, 2, 3 };
        for (var i = 0; i < levelData.RopeCount; i++)
        {
            var rope = Instantiate(ropePrefab);
            
            var index = lengthIndices[Random.Range(0, areaIndices.Count)];
            lengthIndices.Remove(index);
            
            var ropeLength = levelData.RopeLengths[index];
            rope.SetNodeCount(ropeLength);
            
            index = areaIndices[Random.Range(0, areaIndices.Count)];
            areaIndices.Remove(index);
            var area = levelData.AreaSegments[index];
            var x = Random.Range(area.x, area.x + area.width);
            var y = Random.Range(area.y, area.y + area.height);
            rope.FixAt(new Vector2(x, y));
            
            rope.HangItem(hangingItem);
        }
    }

    public void CreateRope(LinkedList<Node> ropeNodes)
    {
        var rope = Instantiate(ropePrefab);
        rope.AddAll(ropeNodes);
    }

    private void Update()
    {
        if (hangingItem.transform.position.y < -8)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
