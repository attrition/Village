using UnityEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    private Map map;

    public int MapSize;

    // Use this for initialization
    void Start()
    {
        map = new Map(MapSize);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
