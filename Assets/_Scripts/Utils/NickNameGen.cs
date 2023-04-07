using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace _Scripts.Utils
{
	public static class NickNameGen
	{
		public async static UniTask<string> GetNickName()
		{
			var req = await UnityWebRequest.Get("https://names.drycodes.com/1").SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				var rawRes = req.downloadHandler.text;
				return rawRes.Substring(2, rawRes.Length - 4).Replace('_', ' ');
			}
			return $"Player {Random.Range(0, 100)}";
		}
	}
}