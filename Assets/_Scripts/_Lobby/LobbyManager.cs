using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Scripts.Utils;
using Cysharp.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Utp;

namespace _Scripts._Lobby
{
	public class LobbyManager : MonoBehaviour
	{
		public Lobby ActiveLobby { get; private set; }
		public event Action<bool> LobbyStatusChanged;
		public event Action<Lobby> LobbyUpdated;
		public static LobbyManager Instance;
		private readonly ConcurrentQueue<string> _createdLobbyIds = new ConcurrentQueue<string>();
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private RelayNetworkManager _relayNetworkManager;
		
		private void Awake()
		{
			if (Instance)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
			_relayNetworkManager = (RelayNetworkManager)NetworkManager.singleton;
		}

		public async UniTask<string> CreateRelay()
		{
			Allocation allocation;
			try
			{
				allocation = await RelayService.Instance.CreateAllocationAsync(4);
			}
			catch (Exception e)
			{
				Debug.LogError($"Relay create allocation request failed {e.Message}");
				throw;
			}
			
			Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
			Debug.Log($"server: {allocation.AllocationId}");
			
			_relayNetworkManager.StartRelayHost(allocation);
			_cts.Cancel();
			_cts.Dispose();
			var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			return joinCode;
		}
		public void JoinRelay(string joinCode)
		{
			try
			{
				_relayNetworkManager.JoinRelayServer(joinCode);
			}
			catch
			{
				Debug.LogError("Relay create join code request failed");
				throw;
			}
		}
		
		private async UniTaskVoid HeartbeatLobby(string lobbyId, float waitTimeSeconds, CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
				await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds), cancellationToken: ct);
			}
		}
		
		private async UniTaskVoid UpdateLobby(string lobbyId, float waitTimeSeconds, CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				ActiveLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
				LobbyUpdated?.Invoke(ActiveLobby);
				await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds), cancellationToken: ct);
			}
		}

		public async UniTaskVoid CreateLobby(string lobbyName, string skinKey)
		{
			try
			{
				print($"Creating lobby {lobbyName}");
				
				var options = new CreateLobbyOptions
				{
					Player = new Player(
						id: AuthenticationService.Instance.PlayerId,
						data: new Dictionary<string, PlayerDataObject>
						{
							{
								Const.PLAYER_SKIN, new PlayerDataObject(
									visibility: PlayerDataObject.VisibilityOptions.Member,
									value: skinKey)
							}
						})
				};
		
				var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
				ActiveLobby = lobby;
				_createdLobbyIds.Enqueue(lobby.Id);
				print($"Lobby {lobby.Id} created!");
				
				HeartbeatLobby(lobby.Id, 15, _cts.Token).Forget();
				UpdateLobby(lobby.Id, 2.5f, _cts.Token).Forget();
				LobbyStatusChanged?.Invoke(true);
			}
			catch (Exception e)
			{
				LobbyStatusChanged?.Invoke(false);
				Debug.LogError(e);
				throw;
			}	
		}

		public async UniTaskVoid JoinToLobby(string lobbyName, string skinKey)
		{
			try
			{
				print($"Connecting to {lobbyName}");
				var options = new JoinLobbyByIdOptions
				{
					Player = new Player(
						id: AuthenticationService.Instance.PlayerId,
						data: new Dictionary<string, PlayerDataObject>
						{
							{
								Const.PLAYER_SKIN, new PlayerDataObject(
									visibility: PlayerDataObject.VisibilityOptions.Member,
									value: skinKey)
							}
						})
				};
				
				//Мини хардкод
				var query = await LobbyService.Instance.QueryLobbiesAsync();
				print($"lobby count {query.Results.Count}");
				var lobbyId = query.Results.First(l => l.Name == lobbyName).Id;
				var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
				
				ActiveLobby = lobby;
				print($"Connected to {lobby.Name}");
				UpdateLobby(lobby.Id, 5, _cts.Token).Forget();
				LobbyStatusChanged?.Invoke(true);
			}
			catch (Exception e)
			{
				LobbyStatusChanged?.Invoke(false);
				Debug.LogError(e);
				throw;
			}
		}
		
		private void OnApplicationQuit()
		{
			if (!_cts.IsCancellationRequested)
			{
				_cts?.Cancel();
				_cts?.Dispose();
			}
			if (ActiveLobby != null)
			{
				LobbyService.Instance.RemovePlayerAsync(ActiveLobby.Id, AuthenticationService.Instance.PlayerId);
			}
			while (_createdLobbyIds.TryDequeue(out var lobbyId))
			{
				LobbyService.Instance.DeleteLobbyAsync(lobbyId);
			}
		}
	}
}