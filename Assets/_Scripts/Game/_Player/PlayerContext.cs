using _Scripts._Lobby;
using _Scripts.Game._Player.Collector;
using _Scripts.Game._Player.Health;
using _Scripts.Utils;
using Mirror;
using Unity.Services.Authentication;
using UnityEngine;

namespace _Scripts.Game._Player
{
	public class PlayerContext : NetworkBehaviour
	{
		[SerializeField, SyncVar] private string _nickName;
		[SerializeField] private PlayerHealth _health;
		[SerializeField] private PlayerMovement _movement;
		[SerializeField] private PlayerSkin _playerSkin;
		[SerializeField] private CoinCollector _coinCollector;

		public string NickName => _nickName;
		public PlayerHealth Health => _health;
		public PlayerMovement Movement => _movement;
		public PlayerSkin PlayerSkin => _playerSkin;
		public CoinCollector CoinCollector => _coinCollector;

		public override void OnStartServer()
		{
			_health.OnDeath += DisablePlayer;
		}

		public override void OnStartClient()
		{
			var me = LobbyManager.Instance.ActiveLobby.Players.Find(player => player.Id == AuthenticationService.Instance.PlayerId);
			_nickName = me.Data[Const.PLAYER_NAME].Value;
		}

		[Server]
		private void DisablePlayer(PlayerHealth obj)
		{
			_movement.Disable();
			_playerSkin.RpcDisable();
		}
	}
}