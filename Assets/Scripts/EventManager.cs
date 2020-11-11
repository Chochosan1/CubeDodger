using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    /// <summary>
    /// Holds custom events.
    /// </summary>
    public class EventManager : MonoBehaviour
    {    
        public delegate void OnPlayerLostDelegate();
        /// <summary>
        /// Fired when the player loses the game.
        /// </summary>
        public static OnPlayerLostDelegate OnPlayerLost;
    }
}
