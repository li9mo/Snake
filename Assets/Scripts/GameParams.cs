using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GameParams",menuName ="Data/GameParams",order =0)]
public class GameParams : ScriptableObject
{
    public Vector2 GridSize = new Vector2(10,10);
    public float CameraToGridSizeRatio = 0.6f;

    public float SnakeSpeed = 1;
    public float SnakeSpeedUpMultiplayer =2f;
    public float SnakeSpeedDownMultiplayer = 0.5f;
    public float DurationForSpeedUp = 1;
    public float DurationForSpeedDown = 1;

    public float TimeForNextEdibleMax = 4;
    public float TimeForNextEdibleMin = 1;

}
