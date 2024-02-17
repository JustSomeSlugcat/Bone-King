﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Bone_King
{
    public static class ExtensionMethods
    {
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
    }

    public class Collider
    {
        private Rectangle area;

        public Rectangle Area 
        { 
            get { return area; } 
            set { area = value; } 
        }
        public Point Offset { get; set; }

        public Collider(Rectangle area, Point offset)
        {
            this.area = area;
            Offset = offset;
        }

        public void Update(Point position) 
        {
            area.X = position.X + Offset.X;
            area.Y = position.Y + Offset.Y;
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

        public PhysicsObject(Vector2 position, Point groundColliderArea, Point groundColliderOffset)
        {
            this.position = position;
            velocity = Vector2.Zero;

            colliders = new List<Collider>{new Collider(new Rectangle(Point.Zero, groundColliderArea), groundColliderOffset)};
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
                colliders[i].Update(new Point((int)position.X, (int)position.Y));
        }
    }
}