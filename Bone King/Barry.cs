﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Emit;

namespace Bone_King
{
    class Barry
    {
        public enum State
        {
            StandingRight,
            StandingLeft,
            RunningRight,
            RunningLeft,
            JumpingRight,
            JumpingLeft,
            Climbing,
            ClimbingOver,
            Death
        }

        public State state;

        Texture2D running, jumping, axe, climbing, climbingOver, death;
        Vector2 position, velocity;
        Rectangle source, feetRectangle, ladderRectangle;
        public Rectangle collision, axeCollision;

        int frameTimer, frame, deathTimer, deathStage, axeSwingTimer, axeTotalTimer, climbedLadder;
        bool feetCollisionCheck, facingRight;
        public bool reset, holdingAxe, axeDown;

        const int ANIMATIONSPEED = 3, LADDERANIMATIONSPEED = 8, DEATHANIMATIONSPEED = 8, AXESPEED = 15, AXETIME = 400;
        const float GRAVITY = 0.1f, MAXFALL = 3.0f, RUNSPEED = 1.5f, JUMPHEIGHT = -2.0f, JUMPSPEED = 1.5f;

        public Barry(Texture2D running, Texture2D axe, Texture2D jumping, Texture2D climbing, Texture2D climbingover, Texture2D death, int x, int y)
        {

            state = State.StandingRight;

            this.running = running;
            this.axe = axe;
            this.jumping = jumping;
            this.climbing = climbing;
            climbingOver = climbingover;
            this.death = death;
            position = new Vector2(x, y);
            velocity = Vector2.Zero;
            source = new Rectangle(0, 0, 32, 32);
            feetRectangle = new Rectangle(x + 4, y + 29, 26, 3);
            ladderRectangle = new Rectangle(x + 4, y + 14, 26, 3);
            collision = new Rectangle(x + 4, y, 26, 32);
            axeCollision = new Rectangle(x - 16, y, 20, 32);

            frameTimer = ANIMATIONSPEED;
            frame = 0;
            deathTimer = 30;
            deathStage = 0;
            axeSwingTimer = AXESPEED;
            axeTotalTimer = AXETIME;

            facingRight = true;
        }

        private void StartLadderClimb(float xPos, float yPos, int ladder)
        {
            frameTimer = 0;
            feetCollisionCheck = false;
            state = State.Climbing;
            velocity.X = 0;
            position.X = xPos;
            position.Y = yPos;
            climbedLadder = ladder;
        }

        public void Update(Input currentInput, Input oldInput, Level level, GameValues gameValues, int maxX, Game1 game)
        {
            if (state != State.Death)
            {
                feetCollisionCheck = false;

                //Gravity
                if (velocity.Y < MAXFALL && state != State.Climbing && state != State.ClimbingOver)
                {
                    velocity.Y += GRAVITY;
                }

                //Checks if player is touching the ground
                for (int i = 0; i < level.platformHitBoxes.Length; i++)
                {
                    if (level.platformHitBoxes[i].Intersects(feetRectangle))
                    {
                        feetCollisionCheck = true;
                        if (velocity.Y >= 0)
                        {
                            velocity.Y = 0;
                            if (state != State.Climbing && state != State.ClimbingOver)
                            {
                                position.Y = level.platformHitBoxes[i].Top - collision.Height + 1;
                            }
                        }
                    }
                }

                //Running and Jumping
                if (feetCollisionCheck && state != State.Climbing && state != State.ClimbingOver)
                {
                    if (currentInput.action && !holdingAxe)
                    {
                        #region Jumping
                        //Jump Left
                        if (currentInput.left)
                        {
                            state = State.JumpingLeft;
                            velocity.Y = JUMPHEIGHT;
                            velocity.X = -JUMPSPEED;
                            facingRight = false;
                        }
                        //Jump Right
                        else if (currentInput.right)
                        {
                            state = State.JumpingRight;
                            velocity.Y = JUMPHEIGHT;
                            velocity.X = JUMPSPEED;
                            facingRight = true;
                        }
                        //Jump Up
                        else if (facingRight)
                        {
                            state = State.JumpingRight;
                            velocity.Y = JUMPHEIGHT;
                            velocity.X = 0;
                        }
                        else
                        {
                            state = State.JumpingLeft;
                            velocity.Y = JUMPHEIGHT;
                            velocity.X = 0;
                        }
                        #endregion
                    }
                    else
                    {
                        #region Running
                        //Run Left
                        if (currentInput.left)
                        {
                            velocity.X = -RUNSPEED;
                            state = State.RunningLeft;
                            facingRight = false;
                        }
                        //Run Right
                        else if (currentInput.right)
                        {
                            velocity.X = RUNSPEED;
                            state = State.RunningRight;
                            facingRight = true;
                        }
                        //Stand Right
                        else if (facingRight)
                        {
                            velocity.X = 0;
                            state = State.StandingRight;
                        }
                        //Stand Left
                        else if (!facingRight)
                        {
                            velocity.X = 0;
                            state = State.StandingLeft;
                        }
                        #endregion
                    }
                }

                #region Climb Ladder
                //Start climbing ladder up or down if infront of ladder and pressing correct button
                if (feetCollisionCheck && state != State.ClimbingOver && !holdingAxe)
                {
                    for (int i = 0; i < level.ladders.Length; i++)
                    {
                        if (currentInput.up && collision.Intersects(level.ladders[i].Body))
                        {
                            StartLadderClimb(level.ladders[i].Body.Center.X - (source.Width / 2), position.Y - 1, i);
                        }
                        if (currentInput.down && feetRectangle.Intersects(level.ladders[i].Top))
                        {
                            StartLadderClimb(level.ladders[i].Top.Center.X - (source.Width / 2), position.Y + 17, i);
                        }
                    }
                }

                //Moves player on the ladder
                if (state == State.Climbing)
                {
                    bool m_climbing = false;

                    if (currentInput.up)
                    {
                        position.Y -= RUNSPEED / 2;
                        m_climbing = true;
                    }
                    if (currentInput.down)
                    {
                        position.Y += RUNSPEED / 2;
                        m_climbing = true;
                    }

                    //Flips sprite left and right while climbing
                    if (m_climbing)
                    {
                        if (frameTimer <= 0)
                        {
                            if (facingRight)
                            {
                                facingRight = false;
                            }
                            else
                            {
                                facingRight = true;
                            }
                            frameTimer = ANIMATIONSPEED * 3;
                        }
                        else
                        {
                            frameTimer -= 1;
                        }
                    }

                    //Moves Barry down if he starts to go above the climbable section of any of the broken ladders
                    if (!collision.Intersects(level.ladders[climbedLadder].Body))
                    {
                        position.Y += RUNSPEED / 2;
                    }

                    //Makes Barry "Climb Over" the platform when he reaches the top of the ladder
                    if (ladderRectangle.Intersects(level.ladders[climbedLadder].Top) && currentInput.up)
                    {
                        state = State.ClimbingOver;
                        frame = 0;
                        frameTimer = LADDERANIMATIONSPEED;
                    }
                    else if (feetCollisionCheck)
                    {
                        state = State.StandingLeft;
                    }
                }
                #endregion

                #region Axe
                //Times the axe swing
                if (axeSwingTimer <= 0)
                {
                    if (axeDown)
                        axeDown = false;
                    else
                        axeDown = true;

                    axeSwingTimer = AXESPEED;
                }
                else
                {
                    axeSwingTimer -= 1;
                }

                //Stops holding axe after a certain amount of time
                if (holdingAxe)
                {
                    if (axeTotalTimer <= 0)
                    {
                        holdingAxe = false;
                        axeTotalTimer = AXETIME;
                    }
                    else
                    {
                        axeTotalTimer -= 1;
                    }
                }
                #endregion

                //Stops Barry from going off the screen
                if (collision.X + collision.Width > maxX)
                {
                    position.X = maxX - collision.Width;
                }
                if (collision.X < 0)
                {
                    position.X = 0;
                }

                //Resets and increases level by 1 each time Barry reaches the goal
                if (collision.Intersects(level.goal) && state != State.Climbing && state != State.ClimbingOver && state != State.JumpingLeft && state != State.JumpingRight)
                {
                    gameValues.level += 1;
                    gameValues.score += (int)game.timer;
                    if (gameValues.level % 5 == 0)
                    {
                        game.lives += 1;
                    }
                    reset = true;
                }

                position += velocity;

                #region Sets size/position of collision rectangles depending on the Animation State
                if (state == State.StandingRight || state == State.StandingLeft)
                {
                    collision = new Rectangle((int)position.X + 4, (int)position.Y, 24, 32);
                    feetRectangle.X = (int)position.X + 4;
                    feetRectangle.Y = (int)position.Y + 29;
                    feetRectangle.Width = 24;
                    ladderRectangle.X = (int)position.X + 4;
                    ladderRectangle.Y = (int)position.Y + 14;
                    ladderRectangle.Width = 24;
                    source.X = 0;
                    if (facingRight)
                    {
                        axeCollision.X = (int)position.X + collision.Width + 4;
                    }
                    else
                    {
                        axeCollision.X = (int)position.X - 16;
                    }
                    axeCollision.Y = (int)position.Y;
                    if (holdingAxe)
                    {
                        if (source.X % 48 != 0)
                        {
                            source.X = 0;
                        }
                    }
                    else
                    {
                        source.Y = 0;
                        if (source.X % 32 != 0)
                        {
                            source.X = 0;
                        }
                    }
                    if (axeDown && holdingAxe)
                    {
                        source.Y = 48;
                    }
                    else
                    {
                        source.Y = 0;
                    }
                    source.Width = 32;
                    source.Height = 32;
                    if (holdingAxe)
                    {
                        source.Width = 48;
                        source.Height = 48;
                    }
                }
                if (state == State.RunningRight || state == State.RunningLeft)
                {
                    collision.X = (int)position.X + 1;
                    collision.Y = (int)position.Y;
                    collision.Width = 30;
                    collision.Height = 32;
                    feetRectangle.X = (int)position.X + 1;
                    feetRectangle.Y = (int)position.Y + 29;
                    feetRectangle.Width = 30;
                    ladderRectangle.X = (int)position.X + 1;
                    ladderRectangle.Y = (int)position.Y + 14;
                    ladderRectangle.Width = 30;
                    if (facingRight)
                    {
                        axeCollision.X = (int)position.X + collision.Width + 1;
                    }
                    else
                    {
                        axeCollision.X = (int)position.X - 19;
                    }
                    axeCollision.Y = (int)position.Y;
                    if (holdingAxe)
                    {
                        if (source.X % 48 != 0)
                        {
                            source.X = 0;
                        }
                    }
                    else
                    {
                        source.Y = 0;
                        if (source.X % 32 != 0)
                        {
                            source.X = 0;
                        }
                    }
                    if (axeDown && holdingAxe)
                    {
                        source.Y = 48;
                    }
                    else
                    {
                        source.Y = 0;
                    }
                    source.Width = 32;
                    source.Height = 32;
                    if (holdingAxe)
                    {
                        source.Width = 48;
                        source.Height = 48;
                    }
                }
                if (state == State.JumpingRight || state == State.JumpingLeft)
                {
                    collision.X = (int)position.X + 2;
                    collision.Y = (int)position.Y;
                    collision.Width = 28;
                    collision.Height = 30;
                    feetRectangle.X = (int)position.X;
                    feetRectangle.Y = (int)position.Y + 29;
                    feetRectangle.Width = 32;
                    ladderRectangle.X = (int)position.X;
                    ladderRectangle.Y = (int)position.Y + 14;
                    ladderRectangle.Width = 32;
                    if (facingRight)
                    {
                        axeCollision.X = (int)position.X + collision.Width + 1;
                    }
                    else
                    {
                        axeCollision.X = (int)position.X - 19;
                    }
                    axeCollision.Y = (int)position.Y;
                    if (source.X % 32 != 0)
                    {
                        source.X = 0;
                    }
                    source.X = 0;
                    source.Y = 0;
                    source.Width = 32;
                    source.Height = 32;
                }
                if (state == State.Climbing)
                {
                    collision.X = (int)position.X + 3;
                    collision.Y = (int)position.Y - 2;
                    collision.Width = 26;
                    collision.Height = 30;
                    feetRectangle.X = (int)position.X + 3;
                    feetRectangle.Y = (int)position.Y + 29;
                    feetRectangle.Width = 26;
                    ladderRectangle.X = (int)position.X + 3;
                    ladderRectangle.Y = (int)position.Y + 14;
                    ladderRectangle.Width = 26;
                    if (facingRight)
                    {
                        axeCollision.X = (int)position.X + collision.Width + 1;
                    }
                    else
                    {
                        axeCollision.X = (int)position.X - 19;
                    }
                    axeCollision.Y = (int)position.Y;
                    if (source.X % 32 != 0)
                    {
                        source.X = 0;
                    }
                    source.X = 0;
                    source.Y = 0;
                    source.Width = 32;
                    source.Height = 32;
                }
                if (state == State.ClimbingOver)
                {
                    collision.X = (int)position.X;
                    collision.Y = (int)position.Y;
                    collision.Width = 32;
                    collision.Height = 32;
                    feetRectangle.X = (int)position.X;
                    feetRectangle.Y = (int)position.Y + 29;
                    feetRectangle.Width = 32;
                    ladderRectangle.X = (int)position.X;
                    ladderRectangle.Y = (int)position.Y + 14;
                    ladderRectangle.Width = 32;
                    if (facingRight)
                    {
                        axeCollision.X = (int)position.X + collision.Width + 1;
                    }
                    else
                    {
                        axeCollision.X = (int)position.X - 19;
                    }
                    axeCollision.Y = (int)position.Y;
                    if (source.X % 32 != 0)
                    {
                        source.X = 0;
                    }
                    source.Y = 0;
                    source.Width = 32;
                    source.Height = 32;
                }
                #endregion
            }

            #region OldInputs
            oldInput.up = currentInput.up;
            oldInput.down = currentInput.down;
            oldInput.left = currentInput.left;
            oldInput.right = currentInput.right;
            oldInput.action = currentInput.action;
            oldInput.pause = currentInput.pause;
#if DEBUG
            oldInput.debug = currentInput.debug;
#endif
            #endregion
        }

        public void Reset(int x, int y)
        {
            state = State.StandingRight;

            position = new Vector2(x, y);
            velocity = Vector2.Zero;
            source = new Rectangle(0, 0, 32, 32);
            feetRectangle = new Rectangle(x + 4, y + 29, 26, 3);
            ladderRectangle = new Rectangle(x + 4, y + 14, 26, 3);
            collision = new Rectangle(x + 4, y, 26, 32);

            frameTimer = ANIMATIONSPEED;
            frame = 0;
            deathTimer = 30;
            deathStage = 0;
            axeSwingTimer = AXESPEED;
            axeTotalTimer = AXETIME;

            facingRight = true;
            reset = false;
            holdingAxe = false;
        }

        public void DeathUpdate(Game1 game)
        {
            if (game.paused == false)
            {
                if (state == State.Death)
                {
                    if (deathStage == 0)
                    {
                        if (deathTimer <= 0)
                        {
                            deathStage = 1;
                            deathTimer = 150;
                            source.X = 32;
                            if (game.deathSongInstance == null)
                            {
                                game.deathSongInstance = game.deathSong.CreateInstance();
                                game.deathSongInstance.Volume = 0.5f;
                                game.deathSongInstance.Play();
                            }
                        }
                        else
                        {
                            deathTimer -= 1;
                        }
                    }
                    if (deathStage == 1)
                    {
                        if (deathTimer <= 0)
                        {
                            deathStage = 2;
                            deathTimer = 150;
                        }
                        else
                        {
                            deathTimer -= 1;
                        }
                    }
                    if (deathStage == 2)
                    {
                        if (deathTimer <= 0)
                        {
                            reset = true;
                        }
                        else
                        {
                            deathTimer -= 1;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch sb, Game1 game)
        {
            #region Draws based on animState
            switch (state)
            {
                case State.StandingRight:
                    if (holdingAxe)
                    {
                        sb.Draw(axe, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, new Vector2(0, 16), 1, SpriteEffects.FlipHorizontally, 0.9f);
                    }
                    else
                    {
                        sb.Draw(running, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0.9f);
                    }
                    break;
                case State.StandingLeft:
                    if (holdingAxe)
                    {
                        sb.Draw(axe, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, new Vector2(20, 16), 1, SpriteEffects.None, 0.9f);
                    }
                    else
                    {
                        sb.Draw(running, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    break;
                case State.RunningRight:
                    if (holdingAxe)
                    {
                        sb.Draw(axe, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, new Vector2(0, 16), 1, SpriteEffects.FlipHorizontally, 0.9f);
                    }
                    else
                    {
                        sb.Draw(running, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0.9f);
                    }
                    if (game.paused == false)
                    {
                        if (frameTimer <= 0)
                        {
                            source.X += source.Width;
                            if (source.X >= running.Width)
                            {
                                source.X = 0;
                            }
                            frameTimer = ANIMATIONSPEED;
                        }
                        else
                        {
                            frameTimer -= 1;
                        }
                    }
                    break;
                case State.RunningLeft:
                    if (holdingAxe)
                    {
                        sb.Draw(axe, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, new Vector2(20, 16), 1, SpriteEffects.None, 0.9f);
                    }
                    else
                    {
                        sb.Draw(running, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    if (game.paused == false)
                    {
                        if (frameTimer <= 0)
                        {
                            source.X += source.Width;
                            if (source.X >= running.Width)
                            {
                                source.X = 0;
                            }
                            frameTimer = ANIMATIONSPEED;
                        }
                        else
                        {
                            frameTimer -= 1;
                        }
                    }
                    break;
                case State.JumpingRight:
                    sb.Draw(jumping, new Vector2((int)position.X, (int)position.Y), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0.9f);
                    break;
                case State.JumpingLeft:
                    sb.Draw(jumping, new Vector2((int)position.X, (int)position.Y), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    break;
                case State.Climbing:
                    if (facingRight)
                    {
                        sb.Draw(climbing, new Vector2((int)position.X, (int)position.Y), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    else
                    {
                        sb.Draw(climbing, new Vector2((int)position.X, (int)position.Y), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0.9f);
                    }
                    break;
                case State.ClimbingOver:
                    sb.Draw(climbingOver, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    if (game.paused == false)
                    {
                        if (frameTimer <= 0)
                        {
                            source.X += source.Width;
                            if (source.X >= climbingOver.Width)
                            {
                                frameTimer = 0;
                                source.X = 0;
                                state = State.StandingLeft;
                                frame = 0;
                            }
                            if (frame == 0 && state != State.StandingLeft)
                            {
                                position.Y -= 4;
                            }
                            if (frame == 1)
                            {
                                position.Y -= 15;
                            }
                            frame += 1;
                            frameTimer = LADDERANIMATIONSPEED;
                        }
                        else
                        {
                            frameTimer -= 1;
                        }
                    }
                    break;
                case State.Death:
                    if (deathStage == 0)
                    {
                        if (source.X % 32 != 0)
                        {
                            source.X = 0;
                        }
                        source.X = 0;
                        source.Y = 0;
                        source.Width = 32;
                        source.Height = 32;
                        sb.Draw(death, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    if (deathStage == 1)
                    {
                        sb.Draw(death, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                        if (game.paused == false)
                        {
                            if (frameTimer <= 0)
                            {
                                source.X += 32;
                                frameTimer = DEATHANIMATIONSPEED;
                                if (source.X >= 160)
                                {
                                    source.X = 32;
                                }
                            }
                            else
                            {
                                frameTimer -= 1;
                            }
                        }
                    }
                    if (deathStage == 2)
                    {
                        source.X = 160;
                        sb.Draw(death, new Vector2((int)position.X, (int)position.Y), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
                    }
                    break;
                default:
                    //panic
                    break;
            }
            #endregion
        }
#if DEBUG
        public void DebugDraw(SpriteBatch sb, Texture2D hitBox, SpriteFont font)
        {
            sb.Draw(hitBox, collision, null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0.999f);
            sb.Draw(hitBox, feetRectangle, null, Color.Blue, 0, Vector2.Zero, SpriteEffects.None, 1);
            sb.Draw(hitBox, ladderRectangle, null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 1);
            sb.Draw(hitBox, axeCollision, null, Color.Green, 0, Vector2.Zero, SpriteEffects.None, 1f);
            sb.DrawString(font, "Axe Time: " + axeTotalTimer, new Vector2(0, 16), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        }
#endif
    }
}
