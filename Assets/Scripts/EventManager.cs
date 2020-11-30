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
        public delegate void OnRequiresNotificationDelegate(UI_Manager.NotificationType notificationType, string messageToDisplay);
        /// <summary>
        /// Fired when an action requires a notification to notify the player - e.g., a new highscore or simply losing the game
        /// </summary>
        public static OnRequiresNotificationDelegate OnRequiresNotification;
    }
}
