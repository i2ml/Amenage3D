using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.ConstraintsScripts
{
    /// <summary>
    ///     If a character element enters in collision with a furniture, outline it in red
    /// </summary>
    public class CollisionChecker : MonoBehaviour
    {
        /// <summary>
        ///     If a character element enters in collision with a furniture, outline it in red
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            OutlineFunctions.SetOutlineEnabled(collision.gameObject, false);
        }

        /// <summary>
        ///     If a character element enters in collision with a furniture, outline it in red
        /// </summary>
        private void OnCollisionStay(Collision collision)
        {
            OutlineFunctions.SetOutlineEnabled(collision.gameObject, true, 2);
        }
    }
}