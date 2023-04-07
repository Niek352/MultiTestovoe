using System;
using System.Collections.Generic;
using _Scripts._Lobby;
using Mirror;
using Unity.Services.Relay.Models;
using Utp;

namespace _Scripts.Network
{
	public class CustomNetworkManager : NetworkManager
	{
		private UtpTransport _utpTransport;
		private IRelayManager _relayManager;
		private int _peopleCountInLobby;
		public event Action<NetworkConnectionToClient> OnServAddPlayer;
		public event Action<NetworkConnectionToClient> OnServerDisconnectPlayer;

		public override void Awake()
		{
			base.Awake();
			_utpTransport = GetComponent<UtpTransport>();
		}
		public override void Start()
		{
			base.Start();
			_relayManager = GetComponent<RelayManager>();
		}

		public void StartRelayHost(Allocation allocation, int peopleCountInLobby)
		{
			_utpTransport.useRelay = true;
			_peopleCountInLobby = peopleCountInLobby;
			_relayManager.ServerAllocation = allocation;
			StartHost();
			OnServAddPlayer += CheckConnectionCount;
		}

		private void CheckConnectionCount(NetworkConnectionToClient conn)
		{
			if (NetworkServer.connections.Count == _peopleCountInLobby)
			{
				OnServAddPlayer -= CheckConnectionCount;
				LobbyManager.Instance.ClearJoinCode();
			}
		}
		
		public void JoinRelayServer(string relayJoinCode)
		{
			_utpTransport.useRelay = true;
			_utpTransport.ConfigureClientWithJoinCode(relayJoinCode,
			StartClient,
			() =>
			{
				UtpLog.Error("Failed to join Relay server.");
			});
		}

		public override void OnServerAddPlayer(NetworkConnectionToClient conn)
		{
			base.OnServerAddPlayer(conn);
			OnServAddPlayer?.Invoke(conn);
		}

		public override void OnServerDisconnect(NetworkConnectionToClient conn)
		{
			OnServerDisconnectPlayer?.Invoke(conn);
			base.OnServerDisconnect(conn);
		}
	}
}