using System.Drawing;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.View
{
   /// <summary>
   /// A class that can take a model and draw to the screen.
   /// </summary>
   public abstract class BaseView<T>
      where T : BaseGameModel
   {
      /// <summary>
      /// Given the model in its current state, draw to the screen.
      /// </summary>
      public abstract void Draw(T model, Painter<T> painter, Graphics g);

      /// <summary>
      /// Given the model in its current state, draw to the screen.
      /// </summary>
      public abstract void DrawOnTopMostLayer(T model, Painter<T> painter, Graphics g);
   }
}
