using Mirror;
using UnityEngine;

namespace _Scripts.Game._Player
{
	public class PlayerMovement : NetworkBehaviour
	{
		[SerializeField] private float _moveSpeed = 5;
		[SerializeField] private float turnSpeed = 2;
		[SyncVar] private bool _isEnabled = true;
		private Vector3 _input;
		private const string HORIZONTAL = "Horizontal";
		private const string VERTICAL = "Vertical";

		
		private void Update()
		{
			if (isLocalPlayer && _isEnabled)
			{
				HandleMovement();
			}
		}

		private void HandleMovement()
		{
			_input = new Vector3(SimpleInput.GetAxisRaw(HORIZONTAL), SimpleInput.GetAxisRaw(VERTICAL), 0);
			if (_input.x != 0 || _input.y != 0)
			{
				transform.position += _input * (_moveSpeed * Time.deltaTime);
				var rotateTarget = Quaternion.LookRotation(Vector3.forward, _input);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTarget, 360 * turnSpeed * Time.deltaTime);
			}
		}
		
		[Server]
		public void Disable()
		{
			_isEnabled = false;
		}
	}
}