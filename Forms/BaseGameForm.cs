/* BaseGameForm.cs
 * 
 * Links:
 * -http://stackoverflow.com/questions/7834775/simple-game-in-c-sharp-with-only-native-libraries
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.Core;

namespace WhiteStone.GameLib.Forms
{
   #region Delegates

   /// <summary>
   /// Used for functions that are meant to clean up when objects are disposed.
   /// </summary>
   public delegate void CleanUpHandler();

   #endregion

   /// <summary>
   /// A base windows form that can be used for games.  Create a new windows form object and inherit from this.
   /// </summary>
   public partial class BaseGameForm : Form
   {
      #region Private Members

      private Bitmap m_buffer;
      private CleanUpHandler m_cleanUpGameCoreHandler;

      #endregion

      #region Constructors

      public BaseGameForm()
      {
         InitializeComponent();
         SetupUnhandledExceptionHandler();
         Load += BaseGameFormLoad;
      }

      #endregion

      #region Public API

      /// <summary>
      /// Supplies the form with a new buffer, representing the current visual state of the world.
      /// Any previous buffer is disposed.
      /// </summary>
      public void AcceptLatestBitmapFrame(Bitmap buffer)
      {
         Bitmap previousBuffer = m_buffer;
         m_buffer = buffer;
         if (previousBuffer != null)
         {
            // Eliminate any previous bitmap.
            previousBuffer.Dispose();
         }
      }

      /// <summary>
      /// Returns how much total size is available for drawing.
      /// </summary>
      public Size GetTotalAvailableSize()
      {
         return new Size(ClientSize.Width, ClientSize.Height);
      }

      #endregion

      #region Internal Methods

      /// <summary>
      /// Installs some necessary callbacks and event handlers into the game core from the game form.
      /// </summary>
      internal void ConfigureGameCoreWithFormCallbacks<T>(GameCore<T> gameCore)
         where T : BaseGameModel
      {
         gameCore.InstallGetTotalDrawingSizeHandler(this.GetTotalAvailableSize);
         gameCore.InstallInvalidateDrawingSurfaceHandler(this.Invalidate);
         gameCore.InstallAcceptLatestBitmapFrameHandler(this.AcceptLatestBitmapFrame);
         gameCore.InstallLocalizeScreenMousePositionHandler(this.ConvertPointToClient);
         gameCore.InstallMainFormHasFocusCb(this.HasFocus);

         m_cleanUpGameCoreHandler = gameCore.CleanUp;
      }


      #endregion

      #region Protected Methods

      /// <summary>
      /// Takes a point which represents any point on the user's display monitor, and converts it to a coordinate local to the form.
      /// </summary>
      protected Point ConvertPointToClient(Point p)
      {
         Point answer = p;
         // This sometimes throws exceptions as we're disposing, and I can't seem to check for that...
         try
         {
            this.Invoke(new Action(() =>
            {
               answer = PointToClient(p);
            }));
         }
         catch
         {
         }
         return answer;
      }

      #endregion

      #region Private Methods

      #region Initialization

      private void SetupGameForm()
      {
         // Prevents flickering during drawing.
         this.SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.DoubleBuffer, true);
      }

      private void SetupGameFormEvents()
      {
         this.Paint += BaseGameFormPaint;
      }

      #endregion

      #region Event Handlers

      private void SetupUnhandledExceptionHandler()
      {
         AppDomain currentDomain = AppDomain.CurrentDomain;
         currentDomain.UnhandledException += CurrentDomainUnhandledException;
      }

      private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
      {
         Exception exc = (Exception)e.ExceptionObject;
         MessageBox.Show(exc.Message);
      }

      /// <summary>
      /// Called when the base form has finished the Load event.
      /// </summary>
      protected virtual void BaseGameFormLoad(object sender, System.EventArgs e)
      {
         SetupGameForm();
         SetupGameFormEvents();
      }

      private void BaseGameFormPaint(object sender, PaintEventArgs e)
      {
         if (m_buffer != null)
         {
            e.Graphics.DrawImageUnscaled(m_buffer, Point.Empty);
         }
      }

      #endregion

      #region Clean Up

      /// <summary>
      /// Cleans up this form during disposal.
      /// </summary>
      private void CleanUp()
      {
         if (m_cleanUpGameCoreHandler != null)
         {
            m_cleanUpGameCoreHandler();
         }
      }

      #endregion

      #region Input

      /// <summary>
      /// Whether the main form has focus.
      /// </summary>
      private Boolean HasFocus()
      {
         Boolean focused = false;
         // This sometimes throws exceptions as we're disposing, and I can't seem to check for that...
         try
         {
            this.Invoke(new Action(() =>
            {
               focused = Focused;
            }));
         }
         catch
         {
         }
         return focused;
      }

      #endregion

      #endregion
   }
}
