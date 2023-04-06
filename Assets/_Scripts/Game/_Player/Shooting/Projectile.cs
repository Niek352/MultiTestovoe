using _Scripts.Game._Player.Health;
using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player.Shooting
{
	public class Projectile : NetworkBehaviour
	{
		[SerializeField] private float _destroyAfter = 2;
		[SerializeField] private Rigidbody2D _rigidBody;
		[SerializeField] private float _force = 100;

		private NetworkIdentity _sender;
		
		[Server]
		public void Init(NetworkIdentity networkIdentity)
		{
			_sender = networkIdentity;
		}
		
		public override void OnStartServer()
		{
			Invoke(nameof(DestroySelf), _destroyAfter);
			AddForce();
		}

		[ContextMenu("AddForce")]
		private void AddForce()
		{
			_rigidBody.AddForce(transform.up * _force);
		}

		[Server]
		private	void DestroySelf()
		{
			NetworkServer.Destroy(gameObject);
		}
		
		[ServerCallback]
		private void OnTriggerEnter2D(Collider2D c)
		{
			if (c.TryGetComponent(out NetworkIdentity identity)
			    && identity.netId != _sender.netId
			    && c.TryGetComponent(out IDamageable damageable))
			{
				damageable.GetDamage(1);
				DestroySelf();
			}
		}
	}
}