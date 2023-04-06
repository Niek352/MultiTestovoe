using Mirror;

namespace _Scripts.Game.Coin
{
	public class GameCoin : NetworkBehaviour
	{
		[Server]
		public void Collect()
		{
			NetworkServer.Destroy(gameObject);
		}
	}
}