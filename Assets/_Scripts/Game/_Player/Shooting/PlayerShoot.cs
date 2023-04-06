using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player.Shooting
{
	public class PlayerShoot : NetworkBehaviour
	{
		[SerializeField] private float _shootCd;
		[SerializeField] private Projectile _projectilePrefab;
		private const string SHOOT = "Shoot";
		private float _timer;
		private bool _buttonPressed;
		
		private void Update()
		{
			if (isLocalPlayer)
			{
				_timer -= Time.deltaTime;
				HandleInput();
				if (_buttonPressed)
				{
					_timer = _shootCd;
					_buttonPressed = false;
					Shoot();					
				}
				
			}
		}
		
		[Command]
		private void Shoot()
		{
			var projectile = Instantiate(_projectilePrefab, transform.position, transform.rotation);
			projectile.Init(netIdentity);
			NetworkServer.Spawn(projectile.gameObject);
		}
		
		private void HandleInput()
		{
			if (_timer <= 0)
			{
				_buttonPressed = SimpleInput.GetButtonDown(SHOOT);
			}
		}
	}
}