using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    [SerializeField] private Rect[] areaSegments = new Rect[4];
    [SerializeField] private int[] ropeLengths;
    [SerializeField] private int ropeCount;

    public Rect[] AreaSegments => areaSegments;
    public int[] RopeLengths => ropeLengths;
    public int RopeCount => ropeCount;
}
