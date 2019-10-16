using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tuth.neabit
{
    public abstract class PlayerMovement
    {
        protected PlayerController player;

        protected PlayerMovement(PlayerController player)
        {
            this.player = player;
        }

        public virtual Status GetStatus()
        {
            return Status.ACTIVE;
        }
        public abstract Vector3 Force();
    }

    public enum Status
    {
        ACTIVE = 1, INACTIVE = 0, REMOVE = -1
    }

    public class DragForce : PlayerMovement
    {
        public DragForce(PlayerController player) : base(player) { }

        public override Vector3 Force()
        {
            const float BASE_DRAG = 95;
            const float DRAG_FACTOR = 20f;
            const float DRAG_THRESHOLD = 90;

            float speed = player.rb.velocity.magnitude;
            if (speed < BASE_DRAG * Time.deltaTime)
            {
                player.rb.velocity = Vector3.zero;
                return Vector3.zero;
            }
            else
            {
                float drag = BASE_DRAG + DRAG_FACTOR * Mathf.Max(0, speed - DRAG_THRESHOLD);
                return -drag * player.rb.velocity.normalized;
            }
        }
    }

    public class ThrustForce : PlayerMovement
    {
        public ThrustForce(PlayerController player) : base(player) { }

        public override Status GetStatus()
        {
            return (player.stunned == 0 && player.inputs.thrust) ? Status.ACTIVE : Status.INACTIVE;
        }

        public override Vector3 Force()
        {
            const float THRUST_FORCE = 190;

            return THRUST_FORCE * player.transform.up;
        }
    }

    public class BoostForce : PlayerMovement
    {
        float boostTime;
        Vector3 direction;

        public BoostForce(PlayerController player, Vector3 direction) : base(player)
        {
            const float BOOST_DURATION = 1f;
            boostTime = BOOST_DURATION;
            this.direction = direction.normalized;
        }

        public override Status GetStatus()
        {
            return (boostTime > 0) ? Status.ACTIVE : Status.REMOVE;
        }

        public override Vector3 Force()
        {
            const float BOOST_SPEED = 1.475f * 95f;
            const float BOOST_ACCEL = 400;

            player.boosting = true;

            if (boostTime == 1f)
            {
                player.rb.velocity += BOOST_SPEED * this.direction;
            }
            boostTime -= Time.deltaTime;
            return Vector3.zero;

            if (player.rb.velocity.magnitude > BOOST_SPEED)
            {
                return Vector3.zero;
            }
            return BOOST_ACCEL * this.direction;
        }
    }
}