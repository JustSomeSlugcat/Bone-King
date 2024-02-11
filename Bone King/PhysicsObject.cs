﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Bone_King
{
    public class Collider
    { 
        public Rectangle area;
        private Vector2 offset;

        public Collider(Rectangle area, Vector2 offset)
        {
            this.area = area;
            this.offset = offset;
        }

        public void Update(Vector2 position) 
        {
            area.X = (int)(position.X + offset.X);
            area.Y = (int)(position.Y + offset.Y);
        }
    }

    public abstract class PhysicsObject
    {
        Sprite sprite;
        Vector2 position, velocity;
        Dictionary<string, Collider> colliders;
        bool grounded;

        float GRAVITY = 0.1f;

        public PhysicsObject(Vector2 position)
        {
            this.position = position;
        }

        protected void UpdatePosition()
        {
            position += velocity;
        }

        protected void CheckGrounded(Rectangle[] platforms)
        {
            grounded = false;
            for (int i = 0; i < platforms.Length; i++)
            {
                if (colliders["ground"].area.Intersects(platforms[i]))
                    grounded = true;
            }
        }

        protected void Gravity()
        {
            if (!grounded)
                velocity.Y += GRAVITY;
        }

        protected void UpdateColliders()
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                colliders.ElementAt(i).Value.Update(position);
            }
        }

        public abstract void Update(Rectangle[] platforms);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
