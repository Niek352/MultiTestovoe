using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _Scripts.Loader
{
	public class Loader : MonoBehaviour
	{
		[SerializeField] private bool isLoggedIn;

		private async UniTaskVoid Start()
		{
			try
			{
				await UnityServices.InitializeAsync();
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);
				isLoggedIn = true;

				SceneLoader.SceneLoader.Instance.LoadLobby().Forget();
			}
			catch (UnityException e)
			{
				isLoggedIn = false;
				Debug.Log(e);
			}
		}
	}
}