using System.Collections.Generic;
using _Scripts._Lobby;
using _Scripts.Utils;
using TMPro;
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
		
		private void Start()
		{
			LobbyManager.Instance.LobbyStatusChanged += LobbyStatusChanged;
			LobbyManager.Instance.LobbyUpdated += UpdateLobby;
			_startGame.onClick.AddListener(StartGame);
			if (LobbyManager.Instance.ActiveLobby != null)
			{
				UpdateLobby(LobbyManager.Instance.ActiveLobby);
				LobbyManager.Instance.PingLobby();
			}
		}
		
		private void StartGame()
		{
			if (!LobbyManager.Instance.IsHost)
				return;
			_startGame.interactable = false;
			LobbyManager.Instance.CreateRelayServer().Forget();
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
			_startGame.gameObject.SetActive(LobbyManager.Instance.IsHost);
			ClearPlayers();

			foreach (var lobbyPlayer in lobby.Players)
			{
				AddPlayerInfo(lobbyPlayer, lobby.Id);
			}
			_container.SetActive(true);
			
			CheckStartGame(lobby);
		}
		
		private void ClearPlayers()
		{
			foreach (var item in _items)
				Destroy(item.gameObject);

			_items.Clear();
		}

		private void CheckStartGame(Lobby lobby)
		{
			if (lobby.Data == null)
				return;
			if (lobby.Data.TryGetValue(Const.JOIN_CODE, out var value) && !string.IsNullOrEmpty(value.Value))
				LobbyManager.Instance.JoinRelay(value.Value);
		}

		

		private void AddPlayerInfo(Player player, string lobbyId)
		{
			var item = Instantiate(_prefabItem, _playersHolder);
			item.Refresh(player, lobbyId);
			_items.Add(item);
		}
	}
}