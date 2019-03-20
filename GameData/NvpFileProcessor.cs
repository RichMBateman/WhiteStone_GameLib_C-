using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteStone.GameLib.GameData
{
   /// <summary>
   /// API that can process a text file (whose format should follow the NVP conventions).
   /// The output from this library is a collection of NvpRecord objects.
   /// </summary>
   public static class NvpFileProcessor
   {
      #region Constants

      /// <summary>
      /// The symbol that divides names and values.
      /// </summary>
      public const Char NameValueDividerSymbol = '=';
      /// <summary>
      /// The character used to mark a data line as being a comment.
      /// </summary>
      public const Char StartOfLineCommentSymbol = '#';
      /// <summary>
      /// The symbol that separates each nvp pair from each other.
      /// </summary>
      public const Char NvpPairSeparator = '#';
      /// <summary>
      /// The character used to indicate an NvpRecordStart.  Note that, before this symbol, is the nvp record type.
      /// </summary>
      public const Char NvpRecordStartSymbol = '|';
      /// <summary>
      /// The symbol that marks the continuation of an Nvp Record.
      /// </summary>
      public const Char NvpContinuationSymbol = '>';
      /// <summary>
      /// For a record, keys may be allowed to repeat; if a repeated key is found, the value is appended to the existing value using this separator.
      /// </summary>
      public const Char NvpDupeKeyValueSeparator = '^';

      #endregion

      #region Public Api

      /// <summary>
      /// Given a collection of strings from a raw NVP file, organizes them into a collection of NVP Records.
      /// This function empties the list supplied to it.
      /// </summary>
      public static List<NvpRecord> CreateNvpRecords(List<String> dataTextLines)
      {
         List<NvpRecord> nvpRecords = new List<NvpRecord>();
         Int32 currentLineIndex = 1;

         while (dataTextLines.Count > 0)
         {
            Int32 endLineIndex;
            List<String> currentBundle;
            CreateLineBundle(dataTextLines, currentLineIndex, out endLineIndex, out currentBundle);
            if (currentBundle.Count > 0) // If the bundle length is 0, the file probably contained only blank lines and comments.
            {
               NvpRecord record = CreateNvpRecordFromBundle(currentBundle, currentLineIndex, endLineIndex);
               nvpRecords.Add(record);
               currentLineIndex = record.LineRangeEnd + 1;
            }
         }
         return nvpRecords;
      }

      #endregion

      #region Private Methods

      #region File Processing

      /// <summary>
      /// Identifies the next set of lines that make up the next NVPRecord.
      /// </summary>
      private static void CreateLineBundle(List<String> dataTextLines, Int32 startLineIndex, out Int32 endLineIndex, out List<String> bundle)
      {
         bundle = new List<String>();

         Int32 lineIndex = 0;
         Boolean recordStartDetected = false;

         while (lineIndex < dataTextLines.Count)
         {
            String currentLine = dataTextLines[lineIndex];
            Boolean isComment = IsLineComment(currentLine);
            if (!isComment) // Completely ignore comment lines.
            {
               Boolean isLineStart = IsLineNvpRecordStart(currentLine);
               Boolean isLineContinuation = IsLineNvpRecordContinuation(currentLine);
               if (!recordStartDetected) // If we haven't yet detected the start of a record...
               {
                  if (isLineStart) // ... and we found one, great!  We are starting the NvpRecord
                  {
                     recordStartDetected = true;
                     bundle.Add(currentLine);
                  }
                  else  // Uh oh... We haven't detected a line start yet, and we're finding something different from a comment or line stop.  HALT!
                  {
                     throw new Exception(String.Format("At line index {0}, Game Data Record Start expected; found: {1}", startLineIndex, currentLine));
                  }
               }
               else // We've already started a record
               {
                  if (isLineContinuation)
                  {
                     bundle.Add(currentLine);
                  }
                  else // We found another record start, OR something bizarro, so stop
                  {
                     dataTextLines.RemoveRange(0, lineIndex);
                     break;
                  }
               }
            }

            if (lineIndex + 1 >= dataTextLines.Count) // if there are no more lines, hack off everything
            {
               dataTextLines.Clear();
            }
            lineIndex++;
         }

         endLineIndex = startLineIndex + lineIndex - 1;
      }

      private static NvpRecord CreateNvpRecordFromBundle(List<String> bundle, Int32 startLine, Int32 endLine)
      {
         NvpRecord record = new NvpRecord();
         record.LineRangeStart = startLine;
         record.LineRangeEnd = endLine;

         if (bundle.Count > 0)
         {
            // First, get the record type.
            Int32 recordTypeIndex = bundle[0].IndexOf(NvpRecordStartSymbol);
            record.RecordType = bundle[0].Substring(0, recordTypeIndex);

            bundle[0] = bundle[0].Substring(recordTypeIndex); // Strip away the record type part of the record.
            while (bundle.Count > 0)
            {
               // At this point, every line either starts with "|" or ">" (for record start, or continuation)
               String currentLine = bundle[0].Substring(1); // Grab all but the first character.
               String[] nvpSet = currentLine.Split(NvpPairSeparator);
               foreach (String nvp in nvpSet)
               {
                  if (nvp.Length > 0) // Blank lines after a | or > are allowed, but basically ignored.
                  {
                     String[] nvpSplit = nvp.Split(NameValueDividerSymbol);
                     String nvpKey = nvpSplit[0];
                     String nvpValue = nvpSplit[1];
                     if (record.NameValuePairs.ContainsKey(nvpKey))
                     {
                        String currentValue = record.NameValuePairs[nvpKey];
                        record.NameValuePairs[nvpKey] = String.Concat(currentValue, NvpDupeKeyValueSeparator, nvpValue);
                     }
                     else
                     {
                        record.NameValuePairs.Add(nvpKey, nvpValue);
                     }
                  }
               }
               bundle.RemoveAt(0); // Remove the first line of the bundle.
            }
         }

         return record;
      }

      #endregion

      #region Text Line Identification

      /// <summary>
      /// Returns true IFF this line is a comment line, and can be safely ignored.
      /// </summary>
      private static Boolean IsLineComment(String line)
      {
         Boolean isComment = true; // Assume the line is a comment.  Demonstrate it's not.

         if (!String.IsNullOrWhiteSpace(line)) // Empty lines are considered comments
         {
            if (!line.StartsWith(StartOfLineCommentSymbol.ToString())) // If the line starts with the comment symbol, it's a comment.
            {
               isComment = false; // Only non-empty lines that don't start with the comment symbol are not comments.
            }
         }

         return isComment;
      }

      /// <summary>
      /// Returns true IFF this text line marks the beginning of a new NVP record.
      /// </summary>
      private static Boolean IsLineNvpRecordStart(String line)
      {
         Boolean isNvpRecordStart = false;

         if (!IsLineComment(line)) // It can't possibly be a record start if the line is a comment.
         {
            isNvpRecordStart = line.Contains(NvpRecordStartSymbol); // This is a loose check.  The symbol can appear multiple times and be treated as part of values.
         }

         return isNvpRecordStart;
      }

      /// <summary>
      /// Returns true IFF this line is a continuation of the previous record.
      /// </summary>
      private static Boolean IsLineNvpRecordContinuation(String line)
      {
         Boolean isContinuation = false;
         if (line.Length >= 1)
         {
            if (line[0] == NvpContinuationSymbol)
            {
               isContinuation = true;
            }
         }
         return isContinuation;
      }

      #endregion

      #endregion
   }
}
