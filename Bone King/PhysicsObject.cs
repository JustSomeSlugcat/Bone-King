﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Bone_King
{
    public class Collider
    {
        private Rectangle area;

        public Rectangle Area 
        { 
            get { return area; } 
            set { area = value; } 
        }
        public Vector2 Offset { get; set; }

        public Collider(Rectangle area, Vector2 offset)
        {
            this.area = area;
            Offset = offset;
        }

        public void Update(Vector2 position) 
        {
            area.X = (int)(position.X + Offset.X);
            area.Y = (int)(position.Y + Offset.Y);
        }
    }

    public abstract class PhysicsObject
    {
        protected Vector2 position, velocity;
        public List<Collider> colliders;
        protected bool grounded, facingRight;
        protected int groundedPlatform;

        protected const float GRAVITY = 0.1f, MAXFALL = 3;

        protected virtual bool DontGround => false;

        public PhysicsObject(Vector2 position, Collider groundCollider)
        {
            this.position = position;
            velocity = Vector2.Zero;

            colliders = new List<Collider>{groundCollider};
        }

        protected virtual void UpdatePosition()
        {
            position += velocity;
        }

        protected void CheckGrounded(Rectangle[] platforms)
        {
            grounded = false;
            for (int i = 0; i < platforms.Length; i++)
            {
                if (!colliders[0].Area.Intersects(platforms[i]))
                    continue;

                grounded = true;

                if (!DontGround && velocity.Y >= 0)
                {
                    velocity.Y = 0;
                    position.Y = platforms[groundedPlatform].Top - colliders[0].Area.Height + 1;
                }

                return;
            }
        }

        protected virtual void Gravity()
        {
            if (!grounded && velocity.Y < MAXFALL)
                velocity.Y += GRAVITY;
        }

        protected void UpdateColliders()
        {
            for (int i = 0; i < colliders.Count; i++)
                colliders[i].Update(position);
        }
    }
}