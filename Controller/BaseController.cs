using WhiteStone.GameLib.Forms;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.Controller
{
   /// <summary>
   /// Base class for game controllers.  Controllers are responsible for taking input and applying it to the model.
   /// Controllers have no sense of the view.  They only know how to manage the model.
   /// </summary>
   /// <typeparam name="T">What kind of GameModel is this for?</typeparam>
   public abstract class BaseController<T>
      where T : BaseGameModel
   {
      /// <summary>
      /// The base behavior is to simply set the CurrentDisplayMousePosition on the model.
      /// </summary>
      public virtual void ApplyInput(T model, UserInput userInput)
      {
         model.CurrentDisplayMousePosition = userInput.GetMousePosition();
      }
   }
}
