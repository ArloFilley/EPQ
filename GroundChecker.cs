using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GroundChecker
{
    public class GroundChecker : MonoBehaviour
    {
        public bool GroundCheck(Transform player, LayerMask whatIsGround, float playerHeight)
        {
            return Physics.Raycast(player.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        }

        public bool OnSlope(Transform player, float maxSlopeAngle, float playerHeight)
        {
            RaycastHit slopeHit;
            if (Physics.Raycast(player.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }
    }   
}

