using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteStone.GameLib.GameData
{
   /// <summary>
   /// A name value pair record is used to group a series of name value pairs (where each name should be unique) to some type of record.
   /// This record is used in conjuncation with my NVP file format.
   /// </summary>
   public class NvpRecord
   {
      #region Private Members

      private readonly Dictionary<String, String> m_nameValuePairs = new Dictionary<String, String>();

      #endregion

      #region Public Properties

      /// <summary>
      /// The record type this belongs to.  RecordType should be a unique key used by the caller to differentiate different types of records.
      /// </summary>
      public String RecordType { get; set; }

      /// <summary>
      /// A collection of name/value pairs.
      /// </summary>
      public Dictionary<String, String> NameValuePairs { get { return m_nameValuePairs; } }

      /// <summary>
      /// The number of the line that starts this record.
      /// </summary>
      public Int32 LineRangeStart { get; set; }

      /// <summary>
      /// The number of the line that ends this record.
      /// </summary>
      public Int32 LineRangeEnd { get; set; }

      #endregion
   }
}
