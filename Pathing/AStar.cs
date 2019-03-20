using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Pathing
{
   /// <summary>
   /// Can calculate shortest paths on an A* map.
   /// </summary>
   public class AStar
   {
      #region Private Members

      private readonly IEqualityComparer<IAStarNode> m_nodeComparer;

      #endregion

      #region Constructors

      public AStar(IEqualityComparer<IAStarNode> nodeComparer)
      {
         m_nodeComparer = nodeComparer;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Returns the best path from start to the goal, where the first entry in the list is the start, and the last is the goal.
      /// Returns null if the goal is not reachable.
      /// </summary>
      public List<IAStarNode> ComputePath(IAStarMap map, IAStarNode start, IAStarNode goal)
      {
         List<IAStarNode> pathFromStartToGoal = ComputePaths(map, start, goal, 1)[0];
         return pathFromStartToGoal;
      }

      /// <summary>
      /// Returns the N best paths from start to goal.  Will only return paths that exist.  If no paths exist, this list will be empty.
      /// </summary>
      public List<List<IAStarNode>> ComputePaths(IAStarMap map, IAStarNode start, IAStarNode goal, Int32 nPaths)
      {
         List<List<IAStarNode>> allPaths = new List<List<IAStarNode>>();

         // http://en.wikipedia.org/wiki/A*_search_algorithm
         List<IAStarNode> pathFromStartToGoal = null;

         HashSet<IAStarNode> closedSet = new HashSet<IAStarNode>(m_nodeComparer);
         HashSet<IAStarNode> openSet = new HashSet<IAStarNode>(m_nodeComparer);
         openSet.Add(start);
         Dictionary<IAStarNode, IAStarNode> cameFromSet = new Dictionary<IAStarNode, IAStarNode>(m_nodeComparer);

         Dictionary<IAStarNode, Double> gScore = new Dictionary<IAStarNode, Double>(m_nodeComparer);
         Dictionary<IAStarNode, Double> fScore = new Dictionary<IAStarNode, Double>(m_nodeComparer);

         gScore.Add(start, 0);
         Double fScoreStart = gScore[start] + map.HeuristicCost(start, goal);
         fScore.Add(start, fScoreStart);


         while (openSet.Count > 0 && nPaths > 0) // While there are still nodes to explore
         {
            IAStarNode current = ReturnINodeInOpenSetWithLowestFScore(openSet, fScore);

            if (current == goal)
            {
               // We've found a path.  Add it to the list.
               pathFromStartToGoal = ConstructPathFromStartToGoal(cameFromSet, current);
               allPaths.Add(pathFromStartToGoal);
               nPaths--; // We have succeeded with one path.
               openSet.Remove(current); // We don't want to add "current" to the openset; we want to backup and keep going.
            }
            else
            {
               openSet.Remove(current);
               closedSet.Add(current);

               List<IAStarNode> neighbors = map.NeighborNodes(current);
               foreach (IAStarNode neighbor in neighbors)
               {
                  if (!closedSet.Contains(neighbor))
                  {
                     Double tentativeGScore = gScore[current] + map.TrueCost(current, neighbor);
                     if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                     {
                        cameFromSet[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        Double fScoreNeighbor = gScore[neighbor] + map.HeuristicCost(neighbor, goal);
                        fScore[neighbor] = fScoreNeighbor;

                        if (!openSet.Contains(neighbor))
                        {
                           openSet.Add(neighbor);
                        }
                     }
                  }
               }
            }
         }

         // We used to return null entries for each Nth path that does not exist.  However, I think this is not useful in real cases.
         //while(nPaths > 0)
         //{
         //    // We promised there'd be one entry per desired path.
         //    allPaths.Add(null);
         //    nPaths--;
         //}

         return allPaths;
      }

      #endregion

      #region Private Methods

      private IAStarNode ReturnINodeInOpenSetWithLowestFScore(HashSet<IAStarNode> openSet, Dictionary<IAStarNode, Double> fScore)
      {
         IAStarNode lowest = null;

         Double smallestFScore = Double.MaxValue;
         foreach (IAStarNode node in openSet)
         {
            Double fScoreForNode = fScore[node];
            if (fScoreForNode < smallestFScore)
            {
               lowest = node;
               smallestFScore = fScoreForNode;
            }
         }

         return lowest;
      }

      private List<IAStarNode> ConstructPathFromStartToGoal(Dictionary<IAStarNode, IAStarNode> cameFromSet, IAStarNode current)
      {
         List<IAStarNode> path = new List<IAStarNode>();
         path.Add(current);

         while (cameFromSet.ContainsKey(current))
         {
            current = cameFromSet[current];
            path.Insert(0, current);
         }

         return path;
      }

      #endregion
   }
}

