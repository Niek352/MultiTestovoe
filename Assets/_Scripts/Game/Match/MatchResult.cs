using _Scripts.Game._Player;
using UnityEngine;

namespace _Scripts.Game.Match
{
	public class MatchResult : MonoBehaviour
	{
		[SerializeField] private Transform _holder;
		[SerializeField] private MatchResultItem _resultItem;
		[SerializeField] private GameObject _container;
		
		public void Show(PlayerContext playerContext)
		{
			_container.SetActive(true);
			var item = Instantiate(_resultItem, _holder);
			item.Init(playerContext.PlayerSkin.Skin, playerContext.CoinCollector.CollectedCoins);
		}
	}

}