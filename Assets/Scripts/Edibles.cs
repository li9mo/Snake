using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class EdibleDM
{
    public EdibleEnum EdibleType;
    public GameObject EdiblePrefab;
}

[CreateAssetMenu(fileName = "EdibleData", menuName = "Data/EdibleData", order = 1)]
public class Edibles : ScriptableObject
{
    public List<EdibleDM> EdiblesList = new List<EdibleDM>();

    public EdibleDM GetRandomEdible()
    {
        return EdiblesList[UnityEngine.Random.Range(0, EdiblesList.Count)];
    }
}
