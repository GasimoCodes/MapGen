using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ALOB.Map
{
    public class Room : ICloneable
    {

        RoomData data;

        public int x, y;

        public Direction angle = Direction.NORTH_UP;
        public List<ulong> connectedRoomsIds;

        public List<ulong> connectedCameras;
        // public List<DoorScript> connectedDoors;
        public GameObject spawnedContainer;



        // Cells which the exits lead to
        public Vector2Int[] localExitCells = new Vector2Int[4];

        // Extension Cells which we occupy
        public Vector2Int[] expandRelativeToOrigin = new Vector2Int[]{};

        public Room(RoomData rD)
        {
            // Copy room data
            data = rD;

            if(data == null)
            {
                MapGenLogger.LogError("Attempt to create an empty room, this is not allowed.");
            }

            // Copy the relativeFromOrigin here so we wont change source assets.
            expandRelativeToOrigin = (Vector2Int[])data.expandRelativeToOrigin.Clone();
            
            refreshExitCells();
        }

        /// <summary>
        /// Assigns local exits, runs only on creation of room.
        /// </summary>
        private void refreshExitCells()
		{
            // Set exit nodes
            switch (getData().shape)
            {

                // Single exit down
                case RoomShapes.ENDROOM:
                    {
                        // Exit is below
                        localExitCells[1] = new Vector2Int(x, y - 1);
                        break;
                    }
                // Hallway 2 exits up down
                case RoomShapes.HALLWAY:
                    {
                        // Exit is below
                        localExitCells[1] = new Vector2Int(x, y - 1);
                        // Exit is above
                        localExitCells[0] = new Vector2Int(x, y + 1);
                        break;
                    }
                // Corner room 2 exits down and right
                case RoomShapes.CROOM:
                    {
                        // Exit is below
                        localExitCells[1] = new Vector2Int(x, y - 1);
                        // Exit is on right
                        localExitCells[3] = new Vector2Int(x + 1, y);
                        break;
                    }
                // T junction room 3 exits left down and right
                case RoomShapes.TSHAPE:
                    {
                        // Exit is below
                        localExitCells[1] = new Vector2Int(x, y - 1);
                        // Exit is on right
                        localExitCells[3] = new Vector2Int(x + 1, y);
                        // Exit is on left
                        localExitCells[2] = new Vector2Int(x - 1, y);
                        break;
                    }
                // Junction 4 exits
                case RoomShapes.FOURDOORS:
                    {
                        // Exit is below
                        localExitCells[1] = new Vector2Int(x, y - 1);
                        // Exit is above
                        localExitCells[0] = new Vector2Int(x, y + 1);
                        // Exit is on right
                        localExitCells[3] = new Vector2Int(x + 1, y);
                        // Exit is on left
                        localExitCells[2] = new Vector2Int(x - 1, y);
                        break;
                    }
            }
        }


        public RoomData getData()
        {
            return data;
        }


        /// <summary>
        /// Reassign variables as if the room was rotated clockwise rotateBy times (!!no negative values!!) 
        /// </summary>
        /// <param name="rotateBy"></param>
        public void RotateClockwise(int rotateBy)
        {

            for (int i = 0; i < Mathf.Abs(rotateBy); i++)
            {

                int currentIndex = 0;
                
                // Rotate blocked rooms when large
                if(getData().large)
                foreach(Vector2Int vInt in expandRelativeToOrigin)
                {
                    // Rotate by 90. Coordinate math tells us [x,y] => [-y,x]
                    Vector2Int tempVInt = VectorUtils.RotateClockwise(vInt);
                    expandRelativeToOrigin[currentIndex] = tempVInt;
                    currentIndex ++;
                }

                currentIndex = 0;

                // Rotate exit cells
                foreach (Vector2Int vInt in localExitCells)
                {
                    // Rotate by 90. Coordinate math tells us [x,y] => [-y,x]
                    Vector2Int tempVInt = VectorUtils.RotateClockwise(vInt);
                    localExitCells[currentIndex] = tempVInt;
                    currentIndex++;
                }


                // Rotate direction variable for identification
                switch (angle)
                {
                    case Direction.NORTH_UP:
                    {
                        angle = Direction.EAST_RIGHT;    
                        break;
                    }
                    case Direction.EAST_RIGHT:
                    {
                        angle = Direction.SOUTH_DOWN;    
                        break;
                    }
                    case Direction.SOUTH_DOWN:
                    {
                        angle = Direction.WEST_LEFT;    
                        break;
                    }
                    case Direction.WEST_LEFT:
                    {
                        angle = Direction.NORTH_UP;    
                        break;
                    }
                }
            }
        }

        public int getAngleInDegrees()
        {
            return VectorUtils.getAngleInDegrees(angle);
        }


		public object Clone()
		{
            return this.MemberwiseClone();
		}

        public override string ToString()
        {
            string tmp =  getData().name + ", (" + x + ", " + y +" " + this.angle + ")";
            return tmp;
        }

    }
}