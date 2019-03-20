namespace WhiteStone.GameLib.Interfaces
{
   /// <summary>
   /// An interface to an object that will accept frame ticks, during which it should do processing.
   /// </summary>
   public interface ITicker
   {
      /// <summary>
      /// Function that is meant to first accept a frame tick.
      /// </summary>
      void AcceptFrameTick();
   }
}
