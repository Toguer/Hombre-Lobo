using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class playerManager : NetworkBehaviour
{
    [SerializeField] private GameObject BackGroundParent;
    [SerializeField] private Image rolImage;
    [SerializeField] private Rol actualRol;
    // Start is called before the first frame update
    void Start()
    {
        GameObject backGround = GameObject.FindGameObjectWithTag("GridBackGround");
        Debug.Log(backGround.name);
        BackGroundParent = backGround;
        try
        {
            transform.SetParent(BackGroundParent.transform);
            if(actualRol is not null)
            {
                rolImage.sprite = actualRol.ImageRol;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        } 


    }

}
