using System;
using System.Collections.Generic;

namespace WhiteStone.GameLib.Sprites
{
   /// <summary>
   /// Spawns sprite instances, which are based off sprite sheets.  Manages the collection of all animations and sprite sheets.
   /// </summary>
   public class SpriteFactory
   {
      #region Private Members

      private readonly Dictionary<String, SpriteSheet> m_spriteSheets = new Dictionary<String, SpriteSheet>();
      private readonly Dictionary<String, List<SpriteSheet>> m_categoryToSpriteSheets = new Dictionary<string, List<SpriteSheet>>();
      private readonly Dictionary<String, Animation> m_animations = new Dictionary<String, Animation>();

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new sprite factory.
      /// </summary>
      public SpriteFactory()
      {
         SetupDefaultAnimation();
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Dictionary of category to a list of sprite sheets.
      /// </summary>
      public Dictionary<String, List<SpriteSheet>> CategoryToSpriteSheets { get { return m_categoryToSpriteSheets; } }

      /// <summary>
      /// Dictionary of sprite sheet key to sprite sheet.
      /// </summary>
      public Dictionary<String, SpriteSheet> SpriteSheets { get { return m_spriteSheets; } }

      /// <summary>
      /// Dictionary of animation key to animation.
      /// </summary>
      public Dictionary<String, Animation> Animations { get { return m_animations; } }

      #endregion

      #region Public Api

      /// <summary>
      /// Adds a new sprite sheet to the collection.
      /// </summary>
      public void AddSpriteSheet(SpriteSheet sheet)
      {
         if (!m_categoryToSpriteSheets.ContainsKey(sheet.Category))
         {
            m_categoryToSpriteSheets[sheet.Category] = new List<SpriteSheet>();
         }
         m_categoryToSpriteSheets[sheet.Category].Add(sheet);
         m_spriteSheets[sheet.Key] = sheet;
         sheet.Animations = m_animations;
      }

      /// <summary>
      /// Adds a new animation to the collection.
      /// </summary>
      public void AddAnimation(Animation animation)
      {
         m_animations[animation.Key] = animation;
      }

      /// <summary>
      /// Returns a brand new sprite based on the given sprite sheet.
      /// </summary>
      /// <param name="spriteSheetKey"></param>
      /// <returns></returns>
      public Sprite SpawnSpriteInstance(String spriteSheetKey)
      {
         SpriteSheet sheet = m_spriteSheets[spriteSheetKey];
         Sprite sprite = new Sprite(sheet);
         return sprite;
      }

      #endregion

      #region Private Methods

      private void SetupDefaultAnimation()
      {
         Animation defaultAnimation = new Animation();
         defaultAnimation.Key = SpriteConstants.DefaultAnimationKey;
         defaultAnimation.AddFrame(0, 0, 0);
         m_animations.Add(defaultAnimation.Key, defaultAnimation);
      }

      #endregion
   }
}
