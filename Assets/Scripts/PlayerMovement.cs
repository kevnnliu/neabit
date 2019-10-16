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
            const float BASE_DRAG = 24;
            const float DRAG_THRESHOLD = 60;
            const float DRAG_FACTOR = (48 - BASE_DRAG) / (95 - DRAG_THRESHOLD);

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
            const float THRUST_FORCE = 48;
            const float LATERAL_DRAG = 2;

            Vector3 lateral = Vector3.ProjectOnPlane(player.rb.velocity, player.transform.up);
            return THRUST_FORCE * player.transform.up - LATERAL_DRAG * lateral;
        }
    }

    public class BoostForce : PlayerMovement
    {
        float boostTime;
        Vector3 direction;

        public BoostForce(PlayerController player, Vector3 direction) : base(player)
        {
            boostTime = 0;
            this.direction = direction.normalized;
        }

        public override Status GetStatus()
        {
            const float BOOST_DURATION = 1f;
            return (boostTime < BOOST_DURATION) ? Status.ACTIVE : Status.REMOVE;
        }

        public override Vector3 Force()
        {
            const float BOOST_DURATION = 0.05f;
            const float BOOST_SPEED = 1.475f * 95f;
            const float LATERAL_DRAG = 0.5f;

            player.boosting = true;

            if (boostTime <= BOOST_DURATION)
            {
                float drag = (boostTime == 0) ? LATERAL_DRAG : 1;
                Vector3 lateral = Vector3.ProjectOnPlane(player.rb.velocity, this.direction);
                player.rb.velocity = BOOST_SPEED * this.direction + drag * lateral;
            }
            boostTime += Time.deltaTime;
            return Vector3.zero;
        }
    }
}