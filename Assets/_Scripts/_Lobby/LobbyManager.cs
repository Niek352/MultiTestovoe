using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Scripts.Network;
using _Scripts.Utils;
using Cysharp.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace _Scripts._Lobby
{
	public class LobbyManager : MonoBehaviour
	{
		public Lobby ActiveLobby { get; private set; }
		public event Action<bool> LobbyStatusChanged;
		public event Action<Lobby> LobbyUpdated;
		public static LobbyManager Instance;
		private readonly ConcurrentQueue<string> _createdLobbyIds = new ConcurrentQueue<string>();
		private CancellationTokenSource _cts = new CancellationTokenSource();
		public bool IsHost => ActiveLobby.HostId == AuthenticationService.Instance.PlayerId;
		private CustomNetworkManager CustomNetworkManager => (CustomNetworkManager)NetworkManager.singleton;
		
		private void Awake()
		{
			if (Instance)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		
		public void PingLobby()
		{
			if (ActiveLobby != null)
			{
				if (!_cts.IsCancellationRequested)
				{
					_cts?.Cancel();
					_cts?.Dispose();
				}
				_cts = new CancellationTokenSource();
				if (IsHost)
				{
					HeartbeatLobby(ActiveLobby.Id, 15, _cts.Token).Forget();
				}
				UpdateLobby(ActiveLobby.Id, 1f, _cts.Token).Forget();
			}
		}

		public async UniTaskVoid CreateRelayServer()
		{
			Allocation allocation;
			try
			{
				allocation = await RelayService.Instance.CreateAllocationAsync(4);
			}
			catch (RelayServiceException e)
			{
				Debug.LogError($"Relay create allocation request failed {e.Message}");
				throw;
			}
			Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
			Debug.Log($"server: {allocation.AllocationId}");
			
			CustomNetworkManager.StartRelayHost(allocation, ActiveLobby.Players.Count);
			if(!_cts.IsCancellationRequested) 
			{
				_cts?.Cancel();
				_cts?.Dispose();
			}
			var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			ActiveLobby = await LobbyService.Instance.UpdateLobbyAsync(ActiveLobby.Id, new UpdateLobbyOptions
			{
				Data = new Dictionary<string, DataObject>
				{
					{
						Const.JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode)
					}
				}
			});
			ActiveLobby.Data[Const.JOIN_CODE] = new DataObject(DataObject.VisibilityOptions.Member, "");
			LobbyUpdated?.Invoke(ActiveLobby);
		}
		
		public async void ClearJoinCode()
		{
			ActiveLobby = await LobbyService.Instance.UpdateLobbyAsync(ActiveLobby.Id, new UpdateLobbyOptions
			{
				Data = new Dictionary<string, DataObject>
				{
					{
						Const.JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, "")
					}
				}
			});
		}

		public void JoinRelay(string joinCode)
		{
			try
			{
				if(!_cts.IsCancellationRequested) 
				{
					_cts?.Cancel();
					_cts?.Dispose();
				}
				CustomNetworkManager.JoinRelayServer(joinCode);
				ActiveLobby.Data[Const.JOIN_CODE] = new DataObject(DataObject.VisibilityOptions.Member, "");
			}
			catch (RelayServiceException e)
			{
				Debug.LogError(e);
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
			var nickName = await NickNameGen.GetNickName();
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
							},
							{
								Const.PLAYER_NAME, new PlayerDataObject(
									visibility: PlayerDataObject.VisibilityOptions.Member,
									value: nickName)
							}
						})
				};
		
				ActiveLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
				_createdLobbyIds.Enqueue(ActiveLobby.Id);
				HeartbeatLobby(ActiveLobby.Id, 15, _cts.Token).Forget();
				UpdateLobby(ActiveLobby.Id, 1f, _cts.Token).Forget();
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
			var nickName = await NickNameGen.GetNickName();
			try
			{
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
							},
							{
								Const.PLAYER_NAME, new PlayerDataObject(
									visibility: PlayerDataObject.VisibilityOptions.Member,
									value: nickName)
							}
						})
				};
				
				var query = await LobbyService.Instance.QueryLobbiesAsync();
				var lobbyId = query.Results.First(l => l.Name == lobbyName).Id;
				var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
				
				ActiveLobby = lobby;
				UpdateLobby(lobby.Id, 1, _cts.Token).Forget();
				LobbyStatusChanged?.Invoke(true);
			}
			catch (Exception e)
			{
				LobbyStatusChanged?.Invoke(false);
				Debug.LogError(e);
				throw;
			}
		}
		private void OnDisable()
		{
			if (!_cts.IsCancellationRequested)
			{
				_cts?.Cancel();
				_cts?.Dispose();
			}
		}
		
		private void OnApplicationQuit()
		{
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