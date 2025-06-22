using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailMarker : MonoBehaviour
{

    [SerializeField] float trailMarkerLife = 10000;

    private int frameCounter = 0;
    // Start is called before the first frame update
    void Start()
    {
        trailMarkerLife += Random.Range(-trailMarkerLife / 2, trailMarkerLife / 2);
    }

    // Update is called once per frame
    void Update()
    {
        frameCounter++;
        if (frameCounter >= trailMarkerLife )
        {
            Destroy(this.gameObject);
        }
    }
}
