using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attached to every object that should interact with the player.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Affects the player in a certain way. A good moment to use this is upon collision.
    /// </summary>
    /// <param name="pc"></param>
    void AffectPlayer(PlayerController pc, bool isCurrentlyMoving);
}
