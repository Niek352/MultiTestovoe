using UnityEngine;

namespace _Scripts.Game._Player.Health
{
	public class HealthBar : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _renderer;
		[SerializeField] private PlayerHealth _playerHealth;
		[SerializeField] private Transform _parent;
		[SerializeField] private float _offset;
		
		private void Awake()
		{
			_playerHealth.OnHealthChanged += HealthChanged;
			transform.SetParent(null);
		}

		private void LateUpdate()
		{
			transform.position = _parent.position + new Vector3(0, _offset);
		}
		
		private void HealthChanged(int currentHealth)
		{
			_renderer.size = new Vector2(Mathf.Clamp01(1f / _playerHealth.MaxHealth * currentHealth), _renderer.size.y);
		}
	}
}