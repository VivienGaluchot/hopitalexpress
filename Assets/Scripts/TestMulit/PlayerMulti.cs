using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerMulti : NetworkBehaviour {

    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
        } else {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .8f);
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 pos) {
        Position.Value = pos;
    }


    void Update() {
        if (IsOwner) {
            Vector2 pos = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) {
                Position.Value = pos;
            } else {
                SubmitPositionRequestServerRpc(pos);
            }
            transform.position = pos;
        } else {
            transform.position = Position.Value;
        }
    }

}
