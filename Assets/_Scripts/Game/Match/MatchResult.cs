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
			item.Init(playerContext.PlayerSkin.Skin, playerContext.CoinCollector.CollectedCoins);
		}

		private void Awake()
		{
			_leaveToLobby.onClick.AddListener(LeaveToLobby);
		}

		private void LeaveToLobby()
		{
			if (NetworkClient.activeHost)
			{
				NetworkManager.singleton.StopHost();
			}
			else
			{
				NetworkManager.singleton.StopClient();
			}
		}
	}

}