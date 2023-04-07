using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Game.Match
{
	public class MatchResultItem : MonoBehaviour
	{
		[SerializeField] private Image _image;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private TextMeshProUGUI _nickName;
		[SerializeField] private string _resultFormat = "Coins collected: {0}";
		
		public void Init(Sprite img, int count, string nickName)
		{
			_image.sprite = img;
			_nickName.text = nickName;
			_text.text = string.Format(_resultFormat, count);
		}
	}
}