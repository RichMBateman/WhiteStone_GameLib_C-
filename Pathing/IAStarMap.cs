using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Pathing
{
   /// <summary>
   /// Represents a map of AStar nodes.
   /// </summary>
   public interface IAStarMap
   {
      Double HeuristicCost(IAStarNode start, IAStarNode goal);
      List<IAStarNode> NeighborNodes(IAStarNode current);
      Double TrueCost(IAStarNode current, IAStarNode adjacent);
   }
}
