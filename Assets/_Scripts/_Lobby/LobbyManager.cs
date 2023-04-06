using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Scripts.Utils;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Scripts._Lobby
{
	public class LobbyManager : MonoBehaviour
	{
		public Lobby ActiveLobby { get; private set; }
		public event Action<bool> LobbyStatusChanged;
		public event Action<Lobby> LobbyUpdated;

		private readonly ConcurrentQueue<string> _createdLobbyIds = new ConcurrentQueue<string>();
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

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