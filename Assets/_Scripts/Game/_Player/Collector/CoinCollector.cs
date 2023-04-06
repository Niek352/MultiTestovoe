using _Scripts.Game.Coin;
using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player.Collector
{
	public class CoinCollector : NetworkBehaviour
	{
		[SerializeField, SyncVar] private int _collectedCoins;

		public int CollectedCoins => _collectedCoins;
		
		[ServerCallback]
		private void OnTriggerEnter2D(Collider2D col)
		{
			if (col.TryGetComponent<GameCoin>(out var coin))
			{
				CollectCoin(coin);
			}
		}
		
		private void CollectCoin(GameCoin coin)
		{
			_collectedCoins++;
			coin.Collect();
		}
	}
}