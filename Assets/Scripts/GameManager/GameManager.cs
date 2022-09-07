using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager _instance;
    public static GameManager Instance => _instance;

    private string _lobbyId;

    private RelayHostData _hostData;
    private RelayJoinData _joinData;


    // Setup events

    //Notificar estado actualizado
    public UnityAction<string> UpdateState;
    //Notificar partida encontrada
    public UnityAction MatchFound;

    private void Awake()
    {
        if(_instance is null)
        {
            _instance = this;

        }
        else
        {
            Destroy(this);
        }
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();

        // Setup Events listeners
        SetupEvents();

        //Unity Login
        await SignInAnonymouslyAsync();

        // Subscribirse a NetworkManager Events
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
    }

    private void ClientConnected(ulong id)
    {
        //Playuer con id se ha conectado a nuestra sesion.

        Debug.Log("Jugador conectado con el id: " + id);

        UpdateState?.Invoke("Jugador encontrado!");
        MatchFound?.Invoke();
    }

    #region UnityLogin

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };
    }


    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
        }
        catch (Exception ex)
        {
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }


    #endregion UnityLogin

    #region Lobby

    public async void FindMatch()
    {
        Debug.Log("Buscando partida...");
        UpdateState?.Invoke("Buscando partida...");
        try
        {
            //Buscando lobby

            // Añadir opciones para el matchMaking
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            //Unir rapido a una partida aleatoria.
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);

            Debug.Log("Unido a la lobby: " + lobby.Id);
            Debug.Log("Jugadores en la lobby: " + lobby.Players.Count);

            string Joincode = lobby.Data["joinCode"].Value;

            Debug.Log("Codigo recibido: " + Joincode);

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(Joincode);

            //crear objeto
            _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            //Set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();

            // Trigger Events
            UpdateState?.Invoke("Partida encontrada!");
            MatchFound?.Invoke();
        }
        catch(LobbyServiceException e)
        {
            //Si no encontramos partida creamos una.
            Debug.Log("No pudiste encontrar partida: " + e);
            CreateMatch();
        }
    }

    private async void CreateMatch()
    {
        Debug.Log("Creando una nueva partida...");
        UpdateState?.Invoke("Creando partida...");
         
        int maxConnections = 1;
        try
        {
            //Creando un Relay Object

            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
            _hostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            // Retrieve JoinCode
            _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            string lobbyName = "game_lobby";
            int maxPLayers = 10;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            // Pon el JoinCode en la información de la lobby, visible para todos los miembros.
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "joinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                    value: _hostData.JoinCode)
                },
            };

            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPLayers, options);
            
            _lobbyId = lobby.Id;
            Debug.Log("Lobby creada: " + lobby.Id);
            // Para no cerrar el server
            StartCoroutine(HeartBeathLobbyCoroutine(lobby.Id, 15));

            //Ahora que el Relay y la lobby estan preparadas

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);
            //Iniciar el host
            NetworkManager.Singleton.StartHost();

            UpdateState?.Invoke("Esperando Jugadores...");
        }
        catch(LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    IEnumerator HeartBeathLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            Debug.Log("Lobby Heartbit");
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        //Necesistamos borrar la lobby que no estamos usando
        Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
    }
    #endregion Lobby

    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    /// <Resumen>
    /// RelayHostData representa la informacion necesaria
    /// para un host para hostear con Relay
    /// </Resumen>
    
    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }
}
