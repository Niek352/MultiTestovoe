using _Scripts.Ui.LobbyUiProvider;
using _Scripts.Utils;
using UnityEngine;

namespace _Scripts.Db
{
	[CreateAssetMenu(menuName = "Settings/" + nameof(PlayerSkinData), fileName = nameof(PlayerSkinData))]
	public class PlayerSkinData : ScriptableObject
	{
		[SerializeField] private LobbyPlayerSkin[] _lobbyPlayerSkin;
		
		private void OnValidate()
		{
			if (_lobbyPlayerSkin.Length > 0)
			{
				_lobbyPlayerSkin[0].Key = Const.DEFAULT_SKIN;
			}
		}

		public Sprite GetSkin(string key)
		{
			foreach (var skin in _lobbyPlayerSkin)
			{
				if (skin.Key == key)
				{
					return skin.Sprite;
				}
			}

			Debug.LogError($"Argument out of range exception key:{key}");
			return null;
		}

		public string NextSkinKey(string currentKey)
		{
			for (int i = 0; i < _lobbyPlayerSkin.Length; i++)
			{
				if (_lobbyPlayerSkin[i].Key == currentKey)
				{
					return i == _lobbyPlayerSkin.Length - 1 ? _lobbyPlayerSkin[0].Key : _lobbyPlayerSkin[i+1].Key;
				}
			}
			
			Debug.LogError($"Argument out of range exception key:{currentKey}");
			return Const.DEFAULT_SKIN;
		}
	}
}