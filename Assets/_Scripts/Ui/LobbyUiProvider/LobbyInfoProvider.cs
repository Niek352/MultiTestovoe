using System;
using System.Collections.Generic;
using _Scripts._Lobby;
using _Scripts.Utils;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Ui.LobbyUiProvider
{
	public class LobbyInfoProvider : MonoBehaviour
	{
		[SerializeField] private GameObject _container;
		[SerializeField] private TextMeshProUGUI _lobbyName;
		[SerializeField] private Transform _playersHolder;
		[SerializeField] private PlayerInfoItem _prefabItem;
		[SerializeField] private Button _startGame;
		private readonly List<PlayerInfoItem> _items = new List<PlayerInfoItem>();
		private bool _isHost;
		
		private void Start()
		{
			LobbyManager.Instance.LobbyStatusChanged += LobbyStatusChanged;
			LobbyManager.Instance.LobbyUpdated += UpdateLobby;
			_startGame.onClick.AddListener(StartGame);
			if (LobbyManager.Instance.ActiveLobby != null)
			{
				UpdateLobby(LobbyManager.Instance.ActiveLobby);
			}
		}

		private async void StartGame()
		{
			if (_isHost)
			{
				var joinCode = await LobbyManager.Instance.CreateRelay();
				await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.ActiveLobby.Id, new UpdateLobbyOptions()
				{
					Data = new Dictionary<string, DataObject>
					{
						{
							Const.JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode)
						}
					}
				});
			}
		}

		private void OnDisable()
		{
			LobbyManager.Instance.LobbyStatusChanged -= LobbyStatusChanged;
			LobbyManager.Instance.LobbyUpdated -= UpdateLobby;
		}

		private void LobbyStatusChanged(bool connected)
		{
			if (connected)
			{
				UpdateLobby(LobbyManager.Instance.ActiveLobby);
			}
			else
			{
				_container.SetActive(false);
			}
		}

		private void UpdateLobby(Lobby lobby)
		{
			_lobbyName.text = lobby.Name;

			if (lobby.HostId == AuthenticationService.Instance.PlayerId)
			{
				_isHost = true;
				_startGame.gameObject.SetActive(true);
			}
			else
				_isHost = false;

			foreach (var item in _items)
				Destroy(item.gameObject);

			_items.Clear();

			foreach (var lobbyPlayer in lobby.Players)
			{
				AddPlayerInfo(lobbyPlayer, lobby.Id);
			}
			_container.SetActive(true);
			
			CheckStartGame(lobby);
		}

		private void CheckStartGame(Lobby lobby)
		{
			if (lobby.Data == null)
				return;
			if (lobby.Data.TryGetValue(Const.JOIN_CODE, out var value))
			{
				LobbyManager.Instance.JoinRelay(value.Value);
			}
		}

		private void AddPlayerInfo(Player player, string lobbyId)
		{
			var item = Instantiate(_prefabItem, _playersHolder);
			item.Refresh(player, lobbyId);
			_items.Add(item);
		}
	}
}