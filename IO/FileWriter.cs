using System;
using System.IO;

namespace WhiteStone.GameLib.IO
{
   public static class FileWriter
   {
      /// <summary>
      /// Writes the text to a file called filename
      /// </summary>
      public static void WriteStringToFile(String fileName, String text, Boolean replace)
      {
         FileMode fileMode = (replace ? FileMode.Create : FileMode.Append);
         FileStream fs = new FileStream(fileName, fileMode);
         StreamWriter sw = new StreamWriter(fs);
         sw.WriteLine(text);
         sw.Close();
         fs.Close();
      }

      /// <summary>
      /// Writes the text to a desktop file called filename
      /// </summary>
      public static void WriteStringToDesktopFile(String fileName, String text, Boolean replace)
      {
         String desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
         String fullFileName = Path.Combine(desktopFolder, fileName);
         WriteStringToFile(fullFileName, text, replace);
      }

      /// <summary>
      /// Writes the text to a desktop file called filename
      /// </summary>
      public static void WriteStringToDesktopFile(String fileName, String text)
      {
         WriteStringToDesktopFile(fileName, text, false);
      }
   }
}
