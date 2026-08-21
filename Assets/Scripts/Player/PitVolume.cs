using UnityEngine;

namespace DoofusDiaries.Player
{
    /// <summary>
    /// Marker component on the (invisible) trigger volume far below the tile
    /// grid. PlayerController checks for the presence of this component --
    /// rather than a string tag or object name -- to recognize "the player
    /// has fallen into the pit" without any fragile string matching.
    /// </summary>
    public class PitVolume : MonoBehaviour
    {
    }
}
