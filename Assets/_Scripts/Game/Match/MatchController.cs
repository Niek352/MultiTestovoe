using System.Collections.Generic;
using System.Linq;
using _Scripts.Game._Player;
using _Scripts.Game._Player.Health;
using _Scripts.Network;
using Mirror;
using UnityEngine;

namespace _Scripts.Game.Match
{
	public class MatchController : NetworkBehaviour
	{
		[SerializeField] private List<PlayerContext> _clients = new List<PlayerContext>();
		[SerializeField] private MatchResult _matchResult;
		private CustomNetworkManager _relayNetworkManager;
		
		public override void OnStartServer()
		{
			_relayNetworkManager = (CustomNetworkManager)NetworkManager.singleton;
			_relayNetworkManager.OnServAddPlayer += OnServerConnected;
			_relayNetworkManager.OnServerDisconnectPlayer += OnServerDisconnected;
			foreach (var conn in NetworkServer.connections.Values)
			{
				ObservePlayer(conn);
			}
		}
		
		[Server]
		private void ObservePlayer(NetworkConnectionToClient connection)
		{
			if (connection.identity.TryGetComponent<PlayerContext>(out var context))
			{
				context.Health.OnDeath += OnPlayerDeath;
				_clients.Add(context);	
			}
		}
		
		private void OnServerDisconnected(NetworkConnectionToClient conn)
		{
			if (conn.identity.TryGetComponent<PlayerContext>(out var context))
			{
				_clients.Remove(context);
			}
		}

		[Server]
		private void OnPlayerDeath(PlayerHealth playerHealth)
		{
			CheckPlayerAlive();
		}
		
		[Server]
		private void CheckPlayerAlive()
		{
			if (_clients.Count(c => !c.Health.IsDead) <= 1)
			{
				var alivePlayer = _clients.FirstOrDefault(p => !p.Health.IsDead);
				EndMatch(alivePlayer);
			}	
		}
		
		[ClientRpc(includeOwner = true)]
		private void EndMatch(PlayerContext playerContext)
		{
			if (playerContext)
			{
				_matchResult.Show(playerContext);
			}
		}

		[Server]
		private void OnServerConnected(NetworkConnectionToClient conn)
		{
			ObservePlayer(conn);
		}
	}
}