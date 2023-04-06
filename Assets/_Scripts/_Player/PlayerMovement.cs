using Mirror;
using UnityEngine;

namespace _Scripts._Player
{
	public class PlayerMovement : NetworkBehaviour
	{
		private void Update()
		{
			if (isLocalPlayer)
			{
				HandleMovement();
			}
		}

		private void HandleMovement()
		{
			float moveHorizontal = Input.GetAxis("Horizontal");
			float moveVertical = Input.GetAxis("Vertical");
			Vector3 movement = new Vector3(moveHorizontal * 0.1f, moveVertical * 0.1f, 0);
			transform.position += movement;
		}
	}
}