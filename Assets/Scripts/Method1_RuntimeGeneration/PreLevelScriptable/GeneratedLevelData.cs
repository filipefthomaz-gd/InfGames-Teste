using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenLevel Data", menuName = "Receive Level Data", order = 52)]
public class GeneratedLevelData : ScriptableObject
{
    public List<GeneratedLevel> genLevels = new List<GeneratedLevel>();
}
