﻿using _Scripts._Lobby;
using _Scripts.Game._Player;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Game.Match
{
	public class MatchResult : MonoBehaviour
	{
		[SerializeField] private Transform _holder;
		[SerializeField] private MatchResultItem _resultItem;
		[SerializeField] private GameObject _container;
		[SerializeField] private Button _leaveToLobby;

		public void Show(PlayerContext playerContext)
		{
			_container.SetActive(true);
			var item = Instantiate(_resultItem, _holder);
			item.Init(playerContext.PlayerSkin.Skin, playerContext.CoinCollector.CollectedCoins, playerContext.NickName);
		}

		private void Start()
		{
			_leaveToLobby.onClick.AddListener(LeaveToLobby);
		}

		private void LeaveToLobby()
		{
			if (LobbyManager.Instance.IsHost)
			{
				NetworkManager.singleton.StopHost();
			}
			else
			{
				NetworkManager.singleton.StartClient();
			}
		}
	}

}