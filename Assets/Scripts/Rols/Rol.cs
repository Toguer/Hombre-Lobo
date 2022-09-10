using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Rols", order = 1)]
public class Rol : ScriptableObject
{
    [SerializeField] private string rolName;
    [SerializeField] private string team;
    [SerializeField] private Sprite imageRol;
    private bool alive = true;

    public Sprite ImageRol { get => imageRol; set => imageRol = value; }

    public void diePlayer()
    {
        alive = false;
    }
}
