using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviour
{
    [SerializeField] private GameObject BackGroundParent;
    // Start is called before the first frame update
    void Start()
    {
        GameObject backGround = GameObject.FindGameObjectWithTag("GridBackGround");
        Debug.Log(backGround.name);
        BackGroundParent = backGround;
        try
        {
            transform.SetParent(BackGroundParent.transform);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        } 


    }

}
