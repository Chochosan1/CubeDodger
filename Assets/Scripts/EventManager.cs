using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class EventManager : MonoBehaviour
    {
        public delegate void OnPlayerLostDelegate();
        public static OnPlayerLostDelegate OnPlayerLost;
    }
}
