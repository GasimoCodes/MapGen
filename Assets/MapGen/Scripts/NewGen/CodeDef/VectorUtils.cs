using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;


namespace ALOB.Map
{
    public static class VectorUtils
    {


        public static Vector2Int translateLocalToGlobal(Vector2Int local, Vector2Int global)
        {
            return new Vector2Int(local.x + global.x, local.y + global.y);
        }

        public static Vector2Int[] translateLocalToGlobal(Vector2Int[] local, Vector2Int global)
        {

            Vector2Int[] temp = new Vector2Int[local.Length];
            int i = 0;

            foreach (Vector2Int vInt in local)
            {
                temp[i] = new Vector2Int(vInt.x + global.x, vInt.y + global.y);
                i++;
            }

            return temp;
        }

        /// <summary>
        /// Returns the Manhattan distance given 2 Coordinates
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static int getManthattanDistance(CellData from, CellData to)
        {
            return Math.Abs(from.loc.x - to.loc.x) + Math.Abs(from.loc.y - to.loc.y);
        }


        /// <summary>
        /// Rotates the given coordinate clockwise once [x,y] => [-y,x]
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The rotated vector</returns>
        public static Vector2Int RotateClockwise(Vector2Int input)
        {
            return new Vector2Int(input.y, -input.x);
        }


        /// <summary>
        /// Returns which direction is "TO" relative to "FROM"
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Direction getRelation(Vector2Int from, Vector2Int to)
        {
            if (from.x != to.x)
            {
                return (from.x > to.x ? Direction.WEST_LEFT : Direction.EAST_RIGHT);
            }
            else
            {
                return (from.y > to.y ? Direction.SOUTH_DOWN : Direction.NORTH_UP);
            }
        }

        /// <summary>
        /// Converts Direction enum to degrees (0-270)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int getAngleInDegrees(Direction d)
        {
            switch (d)
            {
                case Direction.NORTH_UP:
                    {
                        return 0;
                    }

                case Direction.EAST_RIGHT:
                    {
                        return 90;
                    }

                case Direction.SOUTH_DOWN:
                    {
                        return 180;
                    }

                case Direction.WEST_LEFT:
                    {
                        return 270;
                    }
            }

            return 0;

        }

        /// <summary>
        /// Returns the reversed direction enum
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Direction reverseDirection(Direction d)
        {
            switch (d)
            {
                case Direction.NORTH_UP:
                    {
                        return Direction.SOUTH_DOWN;
                    }

                case Direction.EAST_RIGHT:
                    {
                        return Direction.WEST_LEFT;
                    }

                case Direction.SOUTH_DOWN:
                    {
                        return Direction.NORTH_UP;
                    }

                case Direction.WEST_LEFT:
                    {
                        return Direction.EAST_RIGHT;
                    }
            }

            return 0;
        }

        #region Pathfinding 



        /// <summary>
        /// Performs basic BFS from FROM cell to TARGETS cell without prioritization. Returns number of unreachable rooms.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static List<CellData> BFS_CheckReachibility(Vector2Int from, List<Vector2Int> targets, Zone zoneObj)
        {

            //string tars = "";
            //targets.ForEach(x => {tars = tars + x;});

            zoneObj.unMarkTraversed();

            //Debug.Log("<color=yellow>Begin Search for " + from + " to " + tars + "</color> ");

            Queue<CellData> queue = new Queue<CellData>();


            // Set start as visited so its not revisited.
            zoneObj.getCellAt(from).ph_isOnClosedList = true;
            queue.Enqueue(zoneObj.getCellAt(from));

            // Create copy to not modify the source
            List<CellData> targetsCopy = new List<CellData>();
            foreach (Vector2Int v2 in targets)
            {
                targetsCopy.Add(zoneObj.getCellAt(v2));
            }

            // Loop until we BREAK or reach all reachable nodes.
            while (queue.Count > 0)
            {
                // Remove from queue
                CellData cD = queue.Dequeue();

                // Debug.Log("<color=purple>Searching"  + cD.loc  + "</color>");

                // Check if it isnt the target
                for (int i = 0; i < targetsCopy.Count; i++)
                {
                    if (cD.loc == targetsCopy[i].loc)
                    {

                        // Debug.Log("<color=yellow>Target found at"  + cD.loc  + "</color>");

                        targetsCopy.RemoveAt(i);
                        i--;

                        // Cant have more targets in a cell except exits, so we break here.
                        if (cD.containerType != containerType.EXIT)
                        {
                            break;
                        }

                    }
                }

                // Get neighbours
                foreach (CellData next in cD.getReachableNeighbours(true))
                {

                    //Debug.Log("<color=blue> from " + cD.loc +  " -> neighbour"  + next.loc  + "</color> " + next.ph_isOnClosedList + " comp " + zoneObj.getCellAt(next.loc).ph_isOnClosedList);

                    // Queue unvisited nodes
                    if (!next.ph_isOnClosedList)
                    {

                        next.ph_isOnClosedList = true;
                        queue.Enqueue(next);
                    }
                }
            }

            // Reset the traversion
            zoneObj.unMarkTraversed();

            return targetsCopy;
        }


        
        /// <summary>
        /// Performs pathfinding from from to target in the zoneObj
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>Path to target, null if no path exists</returns>
        public static List<CellData> getPath(CellData from, CellData target, Zone zoneObj)
        {



            //Debug.Log("from " + from.loc + " to " + target.loc);

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            List<CellData> openList = new List<CellData>(); // Change to sortedQueue for faster iterations!!
            HashSet<CellData> closedList = new HashSet<CellData>();
            var cameFromPath = new Dictionary<CellData, CellData>();

            // Add start to openList
            from.ph_isOnOpenList = true;
            openList.Add(from);

            // Loop until we BREAK or reach all reachable nodes.
            while (openList.Count > 0)
            {


                // Sort openList
                openList = openList.OrderBy(x => x.ph_fCost).ToList();

                //Debug.Log("OpenList A: " +  openList.Count() + " ClosedList A: " + closedList.Count());

                // Get Cell with lowest FScore from openList
                CellData currentNode = openList[0];
                openList.RemoveAt(0);
                currentNode.ph_isOnOpenList = false;
                // Switch it to closedList
                currentNode.ph_isOnClosedList = true;
                closedList.Add(currentNode);


                // If target, we won.
                if (currentNode.loc == target.loc)
                {
                    string result = "";
                    List<CellData> path = new List<CellData>();
                    CellData current = currentNode;

                    while (current != null)
                    {
                        path.Add(current);

                        result = result + " / " + current.loc;
                        current = current.ph_parent;
                    }

                    path.Reverse();

                    // Debug.Log("<color=purple>A* Path Found for " + zoneObj.name + "</color> " + watch.ElapsedMilliseconds + "ms" + "\n<color=gray>" + "Path: " + result + "</color>");

                    // Reset the traversion
                    foreach (CellData cd in zoneObj.convertMapTo1DArray())
                    {
                        cd.ph_isOnClosedList = false;
                    }

                    return path;
                }



                // For each of currentNode neighbours:
                foreach (CellData child in currentNode.getReachableNeighbours(true))
                {

                    // Debug.Log("Exploring Child of " + currentNode.loc + "/ " + child.loc);

                    // If on closedList, we ignore.
                    if (child.ph_isOnClosedList)
                    {
                        continue;
                    }

                    // If it isnt on the openList
                    if (!child.ph_isOnOpenList)
                    {
                        // Track costs
                        child.ph_parent = currentNode;
                        child.ph_gCost = currentNode.ph_gCost + 1; // +1 is the distance betweem current and child
                        child.ph_hCost = VectorUtils.getManthattanDistance(child, target); // Distance to goal

                        // Add to open list
                        openList.Add(child);
                        child.ph_isOnOpenList = true;
                    }

                    // If it is on the openList 
                    foreach (CellData openNode in openList)
                    {
                        if (child == openNode && child.ph_gCost > openNode.ph_gCost)
                        {
                            openNode.ph_parent = child;
                            openNode.ph_gCost = child.ph_gCost + 1;
                            openNode.ph_hCost = VectorUtils.getManthattanDistance(openNode, target);
                            break;
                        }
                    }
                }
            }

            // Reset the traversion
            foreach (CellData cd in zoneObj.convertMapTo1DArray())
            {
                cd.ph_isOnClosedList = false;
            }

            return null;
        }


        #endregion


    }
}