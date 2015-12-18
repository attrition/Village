using UnityEngine;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    private Map map;

    public int MapSize;

    // Use this for initialization
    void Start()
    {
        map = new Map(this, MapSize);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
