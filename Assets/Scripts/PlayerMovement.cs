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

    public class PlayerStats
    {
        public static float MAX_SPEED = 20;
        public static float BASE_ACCEL = 30;
        public static float BOOST_SPEED = 45;
        public static float BOOST_ACCEL = 100;
        public static float BASE_DRAG = 20;
        public static float DRAG_THRESH = 0.8f;
        public static float LATERAL_DRAG = 5;
        public static float BOOST_LATERAL_DRAG = 20;
    }

    public class DragForce : PlayerMovement
    {
        public DragForce(PlayerController player) : base(player) { }

        public override Vector3 Force()
        {
            float ratio = player.rb.velocity.magnitude / (player.inputs.boosting ? PlayerStats.BOOST_SPEED : PlayerStats.MAX_SPEED);
            float dragFactor = Mathf.Clamp((ratio - PlayerStats.DRAG_THRESH) / (1 - PlayerStats.DRAG_THRESH), 0, 2f);
            float accel = player.inputs.boosting ? PlayerStats.BOOST_ACCEL : PlayerStats.BASE_ACCEL;
            float drag = PlayerStats.BASE_DRAG + accel * dragFactor;
            return -drag * player.rb.velocity.normalized;
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
            Vector3 lateral = Vector3.ProjectOnPlane(player.rb.velocity, player.transform.up);
            return (PlayerStats.BASE_ACCEL + PlayerStats.BASE_DRAG) * player.transform.up - PlayerStats.LATERAL_DRAG * lateral;
        }
    }

    public class BoostForce : PlayerMovement
    {
        public BoostForce(PlayerController player) : base(player) { }

        public override Status GetStatus()
        {
            return (player.stunned == 0 && player.inputs.boosting) ? Status.ACTIVE : Status.INACTIVE;
        }

        public override Vector3 Force()
        {
            Vector3 lateral = Vector3.ProjectOnPlane(player.rb.velocity, player.transform.up);
            return (PlayerStats.BOOST_ACCEL + PlayerStats.BASE_DRAG) * player.transform.up - PlayerStats.BOOST_LATERAL_DRAG * lateral;
        }
    }
}