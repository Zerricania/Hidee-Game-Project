using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public class RoleManager : NetworkBehaviour
{
    private List<NetworkConnection> _players = new List<NetworkConnection>();
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        _players.Clear();
    }

    public void AddPlayer(NetworkConnection conn)
    {
        if (!_players.Contains(conn))
        {
            _players.Add(conn);
        }

        if (_players.Count == 4)
        {
            AssignRoles();
        }
    }

    [Server]
    private void AssignRoles()
    {
        // Выбираем случайного охотника
        NetworkConnection hunter = _players[Random.Range(0, _players.Count)];
        
        foreach (var conn in _players)
        {
            if (conn == hunter)
            {
                TargetSetRole(conn, "Hunter");
            }
            else
            {
                TargetSetRole(conn, "Hider");
            }
        }
    }

    [TargetRpc]
    private void TargetSetRole(NetworkConnection conn, string role)
    {
        Debug.Log($"Твоя роль: {role}");
    }
}