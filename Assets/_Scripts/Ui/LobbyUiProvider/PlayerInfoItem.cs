using System.Collections.Generic;
using _Scripts.Db;
using _Scripts.Utils;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Ui.LobbyUiProvider
{
	public class PlayerInfoItem : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _playerName;
		[SerializeField] private Image _skin;
		[SerializeField] private PlayerSkinData _playerSkinData;
		[SerializeField] private Button _button;

		private string _lobbyId;
		private Player _player;
		
		private void Awake()
		{
			_button.onClick.AddListener(NextSkin);	
		}
		
		private async void NextSkin()
		{
			var key = _player.Data[Const.PLAYER_SKIN];
			var nextSkinKey = _playerSkinData.NextSkinKey(key.Value);
			var sprite = _playerSkinData.GetSkin(nextSkinKey);
			_skin.sprite = sprite;
			_player.Data[Const.PLAYER_SKIN] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nextSkinKey);
			await LobbyService.Instance.UpdatePlayerAsync(_lobbyId, _player.Id, new UpdatePlayerOptions
			{
				Data = new Dictionary<string, PlayerDataObject>()
				{
					{
						Const.PLAYER_SKIN, new PlayerDataObject(
							visibility: PlayerDataObject.VisibilityOptions.Member,
							value: nextSkinKey)
					}
				}
			});
		}

		public void Refresh(Player player, string lobbyId)
		{
			_playerName.text = player.Id;
			_lobbyId = lobbyId;
			_player = player;
			if (AuthenticationService.Instance.PlayerId != player.Id)
				_button.enabled = false;
			if (player.Data.TryGetValue(Const.PLAYER_SKIN, out var key))
			{
				var sprite = _playerSkinData.GetSkin(key.Value);
				_skin.sprite = sprite;
			}
			else
			{
				Debug.LogError("player Skin is out");
			}
		}
	}
}