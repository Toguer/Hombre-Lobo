using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private GameObject playButton;

    void Start()
    {
        //Subscribirse a los eventos
        GameManager.Instance.MatchFound += MatchFound;
        GameManager.Instance.UpdateState += UpdateState;
    }

    private void UpdateState(string newState)
    {
        stateText.text = newState;
    }

    private void MatchFound()
    {
        playButton.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.MatchFound -= MatchFound;
        GameManager.Instance.UpdateState -= UpdateState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
