using System;
using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player.Health
{
	public class PlayerHealth : NetworkBehaviour, IDamageable
	{
		[SerializeField, SyncVar(hook = nameof(HealthHook))] private int _health;
		[SerializeField, SyncVar] private bool _isDead;
		[SerializeField] private int _maxHealth = 10;
		[SerializeField] private GameObject _rip;
		public event Action<int> OnHealthChanged;
		public event Action<PlayerHealth> OnDeath;
		public int MaxHealth => _maxHealth;

		public bool IsDead => _isDead;
		
		public override void OnStartServer()
		{
			_health = _maxHealth;
		}
		
		private void HealthHook(int oldValue, int newValue)
		{
			OnHealthChanged?.Invoke(newValue);
		}
		
		[Server]		
		public void GetDamage(int amount)
		{
			_health -= amount;
			_health = Mathf.Clamp(_health, 0, _maxHealth);
			if (_health == 0)
			{
				ServerDeath();
			}
		}
		
		private void ServerDeath()
		{
			_isDead = true;
			OnDeath?.Invoke(this);
			RpcDeath();
		}

		[ClientRpc(includeOwner = true)]
		private void RpcDeath()
		{
			Instantiate(_rip, transform.position, Quaternion.identity);
		}
	}
}