﻿using System;
using _Scripts._Lobby;
using _Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Ui.LobbyUiProvider
{
	public class LobbyConnectProvider : MonoBehaviour
	{
		[Header("Create Lobby")]
		[SerializeField] private LobbyWidget _createLobby;
		[Header("Join Lobby")]
		[SerializeField] private LobbyWidget _joinLobby;
		private void Start()
		{
			if (LobbyManager.Instance.ActiveLobby != null)
			{
				HideWidgets();
			}
			LobbyManager.Instance.LobbyStatusChanged += LobbyStatusChanged;
			_createLobby.Button.onClick.AddListener(CreateLobby);
			_joinLobby.Button.onClick.AddListener(JoinLobby);
		}
		
		private void OnDisable()
		{
			LobbyManager.Instance.LobbyStatusChanged -= LobbyStatusChanged;
		}

		private void LobbyStatusChanged(bool connected)
		{
			if (connected)
			{
				print("connected");
			}
			else
			{
				ShowWidgets();
			}
		}

		private void HideWidgets()
		{
			_createLobby.Hide();
			_joinLobby.Hide();
		}

		private void ShowWidgets()
		{
			_createLobby.Show();
			_joinLobby.Show();
		}
		
		private void JoinLobby()
		{
			HideWidgets();
			var lobbyName = _joinLobby.InputField.text;
			if (string.IsNullOrEmpty(lobbyName))
			{
				Debug.LogError("Enter Lobby Name");
				return;
			}
			
			LobbyManager.Instance.JoinToLobby(lobbyName, Const.DEFAULT_SKIN).Forget();
		}

		private void CreateLobby()
		{
			HideWidgets();
			var lobbyName = _createLobby.InputField.text;
			if (string.IsNullOrEmpty(lobbyName))
			{
				Debug.LogError("Enter Lobby Name");
				return;
			}
			
			LobbyManager.Instance.CreateLobby(lobbyName, Const.DEFAULT_SKIN).Forget();
		}

		[Serializable]
		public class LobbyWidget
		{
			public GameObject Holder;
			public TMP_InputField InputField;
			public Button Button;

			public void Hide()
			{
				Holder.SetActive(false);
			}

			public void Show()
			{
				Holder.SetActive(true);
			}
		}
	}
}