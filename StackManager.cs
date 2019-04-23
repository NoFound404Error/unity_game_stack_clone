using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    private GameObject[] stacks;
    
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
            stacks[i] = transform.GetChild(i).gameObject;
    }
}
