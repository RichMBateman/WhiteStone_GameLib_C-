using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteStone.GameLib.IO
{
   public static class FileReader
   {
      /// <summary>
      /// Reads from a file.  If no file exists, an empty string will be returned.
      /// </summary>
      public static String ReadFromFile(String filepath)
      {
         String text = String.Empty;
         if (File.Exists(filepath))
         {
            text = File.ReadAllText(filepath);
         }
         return text;
      }
   }
}
