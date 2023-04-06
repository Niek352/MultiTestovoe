using _Scripts._Lobby;
using _Scripts.Db;
using _Scripts.Utils;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;

namespace _Scripts._Player
{
	public class PlayerSkin : NetworkBehaviour
	{
		[SerializeField] private PlayerSkinData _skinData;
		[SerializeField] private SpriteRenderer _renderer;
		[SyncVar(hook = nameof(UpdateSkin))] private string _skinKey;

		
		public override void OnStartLocalPlayer()
		{
			var lobbyPlayer = LobbyManager.Instance.ActiveLobby.Players.Find(player => player.Id == AuthenticationService.Instance.PlayerId);
			_skinKey = lobbyPlayer.Data[Const.PLAYER_SKIN].Value;
			_renderer.sprite = _skinData.GetSkin(_skinKey);
		}
		
		private void UpdateSkin(string keyOld, string keyNew)
		{
			_renderer.sprite = _skinData.GetSkin(keyNew);
		}
	}
}