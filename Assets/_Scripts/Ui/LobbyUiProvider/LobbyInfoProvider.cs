using System;
using System.Collections.Generic;
using _Scripts._Lobby;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Scripts.Ui.LobbyUiProvider
{
	public class LobbyInfoProvider : MonoBehaviour
	{
		[SerializeField] private GameObject _container;
		[SerializeField] private LobbyManager _lobbyManager;
		[SerializeField] private TextMeshProUGUI _lobbyName;
		[SerializeField] private Transform _playersHolder;
		[SerializeField] private PlayerInfoItem _prefabItem;
		
		private readonly List<PlayerInfoItem> _items = new List<PlayerInfoItem>();

		private void Awake()
		{
			_lobbyManager.LobbyStatusChanged += LobbyStatusChanged;
			_lobbyManager.LobbyUpdated += UpdateLobby;
		}

		private void OnDisable()
		{
			_lobbyManager.LobbyStatusChanged -= LobbyStatusChanged;
			_lobbyManager.LobbyUpdated -= UpdateLobby;
		}

		private void LobbyStatusChanged(bool connected)
		{
			if (connected)
			{
				UpdateLobby(_lobbyManager.ActiveLobby);
			}
			else
			{
				_container.SetActive(false);
			}
		}
		
		private void UpdateLobby(Lobby lobby)
		{
			_lobbyName.text = lobby.Name;
			
			foreach (var item in _items)
				Destroy(item.gameObject);
			
			_items.Clear();
			
			foreach (var lobbyPlayer in lobby.Players)
			{
				AddPlayerInfo(lobbyPlayer, lobby.Id);
			}
			_container.SetActive(true);
		}

		private void AddPlayerInfo(Player player, string lobbyId)
		{
			var item = Instantiate(_prefabItem, _playersHolder);
			item.Refresh(player, lobbyId);
			_items.Add(item);
		}
	}
}