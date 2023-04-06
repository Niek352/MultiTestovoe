using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Game.Match
{
	public class MatchResultItem : MonoBehaviour
	{
		[SerializeField] private Image _image;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private string _resultFormat = "Coins collected: {0}";
		
		public void Init(Sprite img, int count)
		{
			_image.sprite = img;
			_text.text = string.Format(_resultFormat, count);
		}
	}
}