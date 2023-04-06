using _Scripts.Game._Player.Collector;
using _Scripts.Game._Player.Health;
using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player
{
	public class PlayerContext : NetworkBehaviour
	{
		[SerializeField] private PlayerHealth _health;
		[SerializeField] private PlayerMovement _movement;
		[SerializeField] private PlayerSkin _playerSkin;
		[SerializeField] private CoinCollector _coinCollector;

		public PlayerHealth Health => _health;
		public PlayerMovement Movement => _movement;
		public PlayerSkin PlayerSkin => _playerSkin;
		public CoinCollector CoinCollector => _coinCollector;

		public override void OnStartServer()
		{
			_health.OnDeath += DisablePlayer;
		}
		
		[Server]
		private void DisablePlayer(PlayerHealth obj)
		{
			_movement.Disable();
			_playerSkin.RpcDisable();
		}
	}
}