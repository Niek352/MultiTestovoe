using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.SceneLoader
{
	public class SceneLoader : MonoBehaviour
	{
		public static SceneLoader Instance;

		private void Awake()
		{
			if (Instance)
				Destroy(Instance);
			Instance = this;
		}

		public async UniTaskVoid LoadLobby()
		{
			await SceneManager.LoadSceneAsync(1);
		}
	}
}