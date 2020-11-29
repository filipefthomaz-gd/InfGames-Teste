using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Create Level Data", order = 51)]
public class LevelData : ScriptableObject
{
    public List<Level> levels = new List<Level>();
}