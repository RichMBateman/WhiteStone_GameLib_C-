using WhiteStone.GameLib.Core;
using WhiteStone.GameLib.Numbers;
using System;
using System.Collections.Generic;
using WhiteStone.GameLib.Model;

namespace WhiteStone.GameLib.Strings
{
   /// <summary>
   /// Class that manages a set of unique names.  Doles them out, and tracks names that have been used or not.
   /// Names are organized into collections by a user-provided String key.
   /// </summary>
   public class NameGenerator
   {
      #region Private Members

      private readonly BaseGameModel m_baseGameModel;
      private readonly Dictionary<String, NameCollection> m_nameCollections = new Dictionary<String, NameCollection>();

      #endregion

      #region Constructors

      public NameGenerator(BaseGameModel model)
      {
         m_baseGameModel = model;
      }

      #endregion

      #region Public Methods

      public void AddName(String key, String name)
      {
         if (!m_nameCollections.ContainsKey(key))
         {
            NameCollection nc = new NameCollection(m_baseGameModel);
            nc.Key = key;
            m_nameCollections.Add(key, nc);
         }
         m_nameCollections[key].AddName(name);
      }

      public String GetName(String key)
      {
         String name = m_nameCollections[key].GenerateName();
         return name;
      }

      public List<String> GetAllRegisteredTypes()
      {
         List<String> allTypes = new List<string>(m_nameCollections.Keys);
         return allTypes;
      }

      public List<String> GetAllNamesForType(String type)
      {
         List<String> allNames = m_nameCollections[type].GetAllNames();
         return allNames;
      }

      #endregion

      #region Private Classes

      private class NameCollection
      {
         #region Private Members

         private readonly BaseGameModel m_baseGameModel;
         private readonly List<String> m_availableNames = new List<String>();
         private readonly List<String> m_usedNames = new List<String>();
         private Int32 recycleCounter = 0;

         #endregion

         #region Constructor

         public NameCollection(BaseGameModel model)
         {
            m_baseGameModel = model;
         }

         #endregion

         #region Public Properties

         public String Key { get; set; }

         #endregion

         #region Public Methods

         public void AddName(String name)
         {
            if (!m_availableNames.Contains(name))
            {
               m_availableNames.Add(name);
            }
         }

         public List<String> GetAllNames()
         {
            List<String> allNames = new List<string>();
            allNames.AddRange(m_availableNames);
            allNames.AddRange(m_usedNames);
            return allNames;
         }

         public String GenerateName()
         {
            if (m_availableNames.Count == 0 && m_usedNames.Count == 0)
            {
               throw new Exception("Name Collection by Key " + Key + " has no names to draw from.");
            }
            if (m_availableNames.Count == 0)
            {
               m_availableNames.AddRange(m_usedNames);
               m_usedNames.Clear();
               recycleCounter++;
            }
            Int32 chosenIndex = RNG.Rnd(0, m_availableNames.Count);
            String generatedName = m_availableNames[chosenIndex];
            if (recycleCounter > 0)
            {
               generatedName = generatedName + recycleCounter;
            }

            m_availableNames.RemoveAt(chosenIndex);
            m_usedNames.Add(generatedName);

            return generatedName;
         }

         #endregion
      }

      #endregion
   }
}
