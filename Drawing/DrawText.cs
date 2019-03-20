using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WhiteStone.GameLib.Model;
using WhiteStone.GameLib.Spatial;

namespace WhiteStone.GameLib.Drawing
{
   /// <summary>
   /// API to assist in drawing text to the screen, and preparing text to be drawn.
   /// </summary>
   public static class DrawText
   {
      #region Public Api

      /// <summary>
      /// Given some text we want to draw (which may not already be set into a PreparedPagedText object),
      /// draw it to the screen at the given logical position.
      /// </summary>
      public static void DrawPreparedText(BaseGameModel model, Position logicalPosition, String fullText, Graphics g, Font font,
         Boolean drawBorder, Pen penBorder, Int32 edgeBuffer, ref PreparedPagedText prepText)
      {
         Position displayPos = model.Camera.MapLogicalToDisplay(logicalPosition, true);
         if (drawBorder)
         {
            Drawing2D.DrawRectBorder(g, penBorder, displayPos);
         }
         if (prepText == null)
         {
            prepText = DrawText.PrepareTextForDisplayInBox(fullText, g,
               font, (Int32)displayPos.Width, (Int32)displayPos.Height, (Int32)(edgeBuffer * model.Camera.CurrentScale));
         }

         List<String> preparedOutputText = prepText.GetCurrentPageText();
         Int32 currentLineY = 0;
         for (Int32 currentLine = 0; currentLine < preparedOutputText.Count; currentLine++)
         {
            String textToDraw = preparedOutputText[currentLine];
            Position displayTextPos = model.Camera.TopLeftAlignTextWithinRect(logicalPosition, textToDraw, font, g, ignoreCameraPosition: true);
            g.DrawString(textToDraw, font, Brushes.White, (Single)displayTextPos.UpperLeftX, (Single)displayTextPos.UpperLeftY + currentLineY);
            currentLineY += prepText.LineHeight;
         }
      }

      /// <summary>
      /// Returns a PreparedPagedText structure that explains how to draw a long piece of text into a rectangular area.
      /// </summary>
      public static PreparedPagedText PrepareTextForDisplayInBox(String textChunk, Graphics g, Font f, Int32 targetBoxWidthPostScale, Int32 targetBoxHeightPostScale, Int32 edgeBuffer)
      {
         PreparedPagedText ppt = new PreparedPagedText();

         // First, we must discount any space that is for an edge buffer.
         Int32 availableWidth = targetBoxWidthPostScale - edgeBuffer;
         Int32 availableHeight = targetBoxHeightPostScale - edgeBuffer;
         Int32 currentAvailableHeight = availableHeight;

         // The text chunk is FIRST divided by any new line characters.  
         String[] remainingTextChunks = textChunk.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
         // We treat these chunks individually.
         foreach (String textElement in remainingTextChunks)
         {
            Boolean finished = false;
            String remainingText = textElement;
            while (!finished)
            {
               SizeF measuredString = g.MeasureString(remainingText, f);
               ppt.LineHeight = (Int32)measuredString.Height;
               if (ppt.LineHeight > currentAvailableHeight)
               {
                  // We will never have enough line height, so end, now.
                  finished = true;
               }
               else
               {
                  // If the width of the remaining text is less than the desired textbox width, we're done; 
                  // there's nothing more to split.  The while loop ALWAYS ends here; if the text is bigger,
                  // then we KNOW there will be another line.  Eventually, the last line will be less than the available width.
                  if (measuredString.Width < availableWidth)
                  {
                     AddLineToPreparedTextWithPageCheck(ppt, remainingText, availableHeight, ref currentAvailableHeight);
                     finished = true;
                  }
                  else
                  {
                     // If the remaining text is too long for this line, keep building up a new line, one word at a time,
                     // until it goes over the target width.  When it does, remove the last added word, and add that line.
                     // Then, build up the remaining text. 
                     String[] wordArray = remainingText.Split(' ');
                     if (wordArray.Length == 1)
                     {
                        // If after splitting, we have only ONE entry, we will never be able to finish.  We will never be able to fit this line in the provided box.  So bail out.
                        // Otherwise we'd get stuck in an infinite loop.
                        finished = true;
                     }
                     else
                     {
                        remainingText = ""; // Empty the raw text string.  Anything that goes here will be more text to process.
                        String textLine = ""; // The line of text that will be safe to add
                        StringBuilder sbTextLineTemp = new StringBuilder(); // Text plus the extra word 
                        Boolean buildingRemainingText = false;
                        StringBuilder sbRemainingText = new StringBuilder();

                        for (Int32 currentWordIndex = 0; currentWordIndex < wordArray.Length; currentWordIndex++)
                        {
                           if (buildingRemainingText) /* i.e., we're preparing the remaining text for the next go around of this process. */
                           {
                              sbRemainingText.Append(wordArray[currentWordIndex]);
                              if (currentWordIndex < wordArray.Length - 1) { sbRemainingText.Append(" "); }
                           }
                           else
                           {
                              sbTextLineTemp.Append(wordArray[currentWordIndex]);
                              if (currentWordIndex + 1 < wordArray.Length) { sbTextLineTemp.Append(" "); }
                              measuredString = g.MeasureString(sbTextLineTemp.ToString(), f);
                              if (measuredString.Width >= availableWidth) /* We've gone overboard; we don't want the last word we added. */
                              {
                                 AddLineToPreparedTextWithPageCheck(ppt, textLine, availableHeight, ref currentAvailableHeight);
                                 sbRemainingText.Append(wordArray[currentWordIndex]);
                                 if (currentWordIndex < wordArray.Length - 1) { sbRemainingText.Append(" "); }
                                 buildingRemainingText = true;
                              }
                              else
                              {
                                 textLine = sbTextLineTemp.ToString();
                              }
                           }
                        }

                        remainingText = sbRemainingText.ToString();
                     }
                  }
               }
            }
         }

         ppt.CurrentPageNumber = 0;

         return ppt;
      }

      /// <summary>
      /// Given some offset rectangle, and some logical position, draw a solid color rectangle and text over it.
      /// </summary>
      public static void DrawTextOnRectangle(BaseGameModel model, Position logPosOffsetRect, Position logPosRect,
         SolidBrush sbBaseRect, SolidBrush sbText, Graphics g, Font f, String text)
      {
         logPosRect = logPosRect.AddPositionUpperLeft(logPosOffsetRect);
         Position disPosRect = model.Camera.MapLogicalToDisplay(logPosRect, true);
         Drawing2D.DrawRect(g, sbBaseRect, disPosRect);
         Position disPosText = model.Camera.BottomLeftAlignTextWithinRect(logPosRect, text, f, g, true);
         g.DrawString(text, f, sbText, (Single)disPosText.UpperLeftX, (Single)disPosText.UpperLeftY);
      }

      #endregion

      #region Private Methods

      private static void AddLineToPreparedTextWithPageCheck(PreparedPagedText ppt, String line, Int32 availableHeight, ref Int32 currentAvailableHeight)
      {
         ppt.AddLineToPage(ppt.CurrentPageNumber, line);
         currentAvailableHeight -= ppt.LineHeight;
         if (currentAvailableHeight < ppt.LineHeight)
         {
            currentAvailableHeight = availableHeight;
            ppt.CurrentPageNumber++;
         }
      }

      #endregion
   }
}
