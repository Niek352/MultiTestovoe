using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _Scripts.Loader
{
	public class Loader : MonoBehaviour
	{
		private async UniTaskVoid Start()
		{
			try
			{
				await UnityServices.InitializeAsync();
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);

				SceneLoader.SceneLoader.Instance.LoadLobby().Forget();
			}
			catch (UnityException e)
			{
				Debug.LogError(e);
			}
		}
	}
}