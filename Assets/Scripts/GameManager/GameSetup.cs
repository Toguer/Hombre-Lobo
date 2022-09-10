using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup gameInstance;
    [SerializeField] private int wolves;
    public List<Rol> rolList;
    public List<playerManager> playerList;
    public delegate void OnGameStart();
    public static OnGameStart onGameStart;



    private void Awake()
    {
        if (gameInstance is null)
        {
            gameInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        playerList = new List<playerManager>();
    }

    public void startGame()
    {
        onGameStart.Invoke();

        rolList.Shuffle();
    }

    public void addPlayer(playerManager playerManager)
    {
        playerList.Add(playerManager);
    }



}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rnd = new System.Random();
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.Next(i, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}