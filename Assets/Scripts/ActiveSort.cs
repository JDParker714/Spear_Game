using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSort : MonoBehaviour
{
    public bool root = false;
    public int offset = 0;
    private SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        if(rend!=null)
        {
            if(root)
            {
                rend.sortingOrder = Mathf.RoundToInt(-100 * transform.root.position.y) + offset;
            }
            else
            {
                rend.sortingOrder = Mathf.RoundToInt(-100 * transform.position.y) + offset;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rend != null)
        {
            if (root)
            {
                rend.sortingOrder = Mathf.RoundToInt(-100 * transform.root.position.y) + offset;
            }
            else
            {
                rend.sortingOrder = Mathf.RoundToInt(-100 * transform.position.y) + offset;
            }
        }
    }
}
