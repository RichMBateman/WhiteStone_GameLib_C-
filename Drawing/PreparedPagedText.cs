using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteStone.GameLib.Drawing
{
   /// <summary>
   /// Used to hold text that has been prepared for display within some rectangular area.
   /// </summary>
   public class PreparedPagedText
   {
      #region Constructors

      public PreparedPagedText()
      {
         CurrentPageNumber = 0;
         FormattedOutput = new Dictionary<int, List<String>>();
         EnsurePageExists(CurrentPageNumber);
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Refers to what page the user is viewing of the prepared text.
      /// </summary>
      public Int32 CurrentPageNumber { get; set; }

      /// <summary>
      /// A mapping of page numbers to textual output, where each entry should fit on one line.
      /// </summary>
      public Dictionary<Int32, List<String>> FormattedOutput { get; set; }

      /// <summary>
      /// The height of text lines.
      /// </summary>
      public Int32 LineHeight { get; set; }

      #endregion

      #region Public Api

      /// <summary>
      /// Advances to next page if there is one.
      /// </summary>
      public void NextPage()
      {
         CurrentPageNumber++;
         if (CurrentPageNumber >= FormattedOutput.Count)
         {
            CurrentPageNumber = FormattedOutput.Count - 1;
         }
      }

      /// <summary>
      /// Goes to previous page.  Loops if there are no more pages.
      /// </summary>
      public void PreviousPage()
      {
         CurrentPageNumber--;
         if (CurrentPageNumber < 0)
         {
            CurrentPageNumber = 0;
         }
      }

      /// <summary>
      /// Adds a line to a page.
      /// </summary>
      public void AddLineToPage(Int32 pageNumber, String line)
      {
         EnsurePageExists(pageNumber);
         List<String> pageContents = FormattedOutput[pageNumber];
         pageContents.Add(line);
      }

      /// <summary>
      /// Returns the text for the current page.
      /// </summary>
      public List<String> GetCurrentPageText()
      {
         List<String> linesOfText = FormattedOutput[CurrentPageNumber];
         return linesOfText;
      }

      #endregion

      #region Private Methods

      private void EnsurePageExists(Int32 pageNumber)
      {
         if (!FormattedOutput.ContainsKey(pageNumber))
         {
            FormattedOutput.Add(pageNumber, new List<String>());
         }
      }

      #endregion
   }
}
