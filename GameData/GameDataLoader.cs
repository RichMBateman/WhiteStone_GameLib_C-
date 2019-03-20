using WhiteStone.GameLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using WhiteStone.GameLib.Model;


namespace WhiteStone.GameLib.GameData
{
   #region Delegates

   /// <summary>
   /// Processes an NVP record for a record type.
   /// </summary>
   public delegate void RecordTypeProcessRecord(BaseGameModel model, NvpRecord record);
   /// <summary>
   /// Function for reporting back to the GameDataLoader object progress.
   /// </summary>
   internal delegate void ReportGameDataLoaderProgress(String message, Int32 resourcesLoaded, Int32 resourcesToLoad);

   #endregion

   /// <summary>
   /// Object that's responsible for loading game data files.  Keeps track of what's been loaded thus far, and applies these data files
   /// to the actual model.
   /// </summary>
   public class GameDataLoader
   {
      #region Constants

      /// <summary>
      /// The default name of the directory containing game data.
      /// </summary>
      private const String DefaultDirectoryData = @"assets\data";
      /// <summary>
      /// The default pattern for game data files.
      /// </summary>
      private const String DefaultDataSearchPattern = "*.txt";
      /// <summary>
      /// Default simulated loading delay.
      /// </summary>
      private const Int32 DefaultSimulatedLoadingDelay = 0;

      #endregion

      #region Private Members

      private readonly BaseGameModel m_gameModel;
      private readonly BackgroundWorker m_resourceLoader = new BackgroundWorker();
      private readonly Dictionary<String, Int32> m_recordTypeToSortValue = new Dictionary<String, Int32>();
      private readonly Dictionary<String, RecordTypeProcessRecord> m_recordTypeToProcessCallback = new Dictionary<String, RecordTypeProcessRecord>();

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new GameDataLoader.
      /// </summary>
      internal GameDataLoader(BaseGameModel gameModel)
      {
         m_gameModel = gameModel;
         DirectoryData = DefaultDirectoryData;
         DataFileSearchPattern = DefaultDataSearchPattern;
         SimulatedLoadingDelay = DefaultSimulatedLoadingDelay;
      }

      #endregion

      #region Public Properties

      #region Loading Behavior

      /// <summary>
      /// When greater than 0, simulates a delay between loading elements of data.
      /// </summary>
      public Int32 SimulatedLoadingDelay { get; set; }

      /// <summary>
      /// Specifies the order in which record types should be processed.
      /// </summary>
      private Dictionary<String, Int32> RecordTypeToSortValue
      {
         get { return m_recordTypeToSortValue; }
      }

      #endregion

      #region Data Directory

      /// <summary>
      /// The directory to search for all game data files.
      /// </summary>
      public String DirectoryData { get; set; }

      /// <summary>
      /// The pattern to use when searching for game data files.
      /// </summary>
      public String DataFileSearchPattern { get; set; }

      #endregion

      #region Resource Counts and Status

      /// <summary>
      /// Whether to show the resource counts.
      /// </summary>
      public Boolean ShowResourceCounts { get; set; }

      /// <summary>
      /// A number that tracks the number of resources loaded so far.
      /// </summary>
      public Int32 ResourcesLoaded { get; set; }

      /// <summary>
      /// The number of resources left to load.  May change as we learn of more things to learn.
      /// </summary>
      public Int32 ResourcesToLoad { get; set; }

      /// <summary>
      /// A message to show to the user.
      /// </summary>
      public String CurrentStatusMessage { get; set; }

      #endregion

      #endregion

      #region Public Api

      /// <summary>
      /// Kicks off the loading of all needed data.
      /// </summary>
      internal void Start()
      {
         ShowResourceCounts = false;
         SetupBackgroundWorker();
      }

      /// <summary>
      /// Loads all the data immediately in the current thread.  Useful for test applications.
      /// </summary>
      public void LoadGameDataInThisThread()
      {
         PerformLoadOperation();
      }

      /// <summary>
      /// Installs a record type into the game data loader.  When records of this type are encountered, they can be processed
      /// with the supplied callback, and records of this type will be sorted in the supplied order.
      /// </summary>
      public void InstallRecordType(String recordType, Int32 sortOrder, RecordTypeProcessRecord processCb)
      {
         m_recordTypeToSortValue.Add(recordType, sortOrder);
         m_recordTypeToProcessCallback.Add(recordType, processCb);
      }

      private void ProcessNvpRecords(List<NvpRecord> nvpRecords, ReportGameDataLoaderProgress reportCb)
      {
         Int32 recordsProcessed = 0;
         foreach (NvpRecord record in nvpRecords)
         {
            reportCb("Processing game data entries...", recordsProcessed, nvpRecords.Count);
            if (m_recordTypeToProcessCallback.ContainsKey(record.RecordType))
            {
               RecordTypeProcessRecord processCb = m_recordTypeToProcessCallback[record.RecordType];
               processCb(m_gameModel, record);
            }
            else
            {
               throw new Exception(String.Format("GameDataApplier does not know how to process a record type of {0} for lines {1} - {2}",
                  record.RecordType, record.LineRangeStart, record.LineRangeEnd));
            }
            SimulateLoadingDelay();
            recordsProcessed++;
         }
      }


      #endregion

      #region Private Methods

      #region Background Worker

      private void SetupBackgroundWorker()
      {
         m_resourceLoader.DoWork += ResourceLoaderDoWork;
         m_resourceLoader.RunWorkerCompleted += ResourceLoaderRunWorkerCompleted;
         m_resourceLoader.RunWorkerAsync();
      }

      private void ResourceLoaderRunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
      {
         //m_gameModel.MarkLoadingComplete();
      }

      private void ResourceLoaderDoWork(Object sender, DoWorkEventArgs e)
      {
         PerformLoadOperation();
      }

      private void PerformLoadOperation()
      {
         SimulateLoadingDelay();
         List<String[]> dataFileContents = GetAllGameDataFiles();
         SimulateLoadingDelay();
         List<NvpRecord> nvpRecords = ProcessGameDataFileContents(dataFileContents);
         SimulateLoadingDelay();
         ProcessNvpRecords(nvpRecords, ReportInitializationDataProgress);
         SimulateLoadingDelay();
      }

      #endregion

      #region Data Files

      private List<String[]> GetAllGameDataFiles()
      {
         List<String[]> fileContents = new List<String[]>();
         if (Directory.Exists(DirectoryData))
         {
            String[] filePaths = Directory.GetFiles(DirectoryData, DataFileSearchPattern);

            ShowResourceCounts = true;
            ResourcesToLoad = filePaths.Length;
            ResourcesLoaded = 0;
            CurrentStatusMessage = "Loading game data files...";

            for (Int32 fileIndex = 0; fileIndex < filePaths.Length; fileIndex++)
            {
               String filePath = filePaths[fileIndex];
               String[] contents = File.ReadAllLines(filePath);
               fileContents.Add(contents);
               ResourcesLoaded++;
               SimulateLoadingDelay();
            }
         }

         return fileContents;
      }

      private List<NvpRecord> ProcessGameDataFileContents(List<String[]> dataFileContents)
      {
         ShowResourceCounts = true;
         ResourcesToLoad = dataFileContents.Count;
         ResourcesLoaded = 0;
         CurrentStatusMessage = "Processing game data files...";

         List<NvpRecord> allNvpRecords = new List<NvpRecord>();
         foreach (String[] fileContents in dataFileContents)
         {
            List<NvpRecord> nvpRecords = NvpFileProcessor.CreateNvpRecords(new List<String>(fileContents));
            allNvpRecords.AddRange(nvpRecords);
            ResourcesLoaded++;
            SimulateLoadingDelay();
         }

         // Sort the nvp records according to their record type.
         allNvpRecords.Sort(delegate (NvpRecord record1, NvpRecord record2)
         {
            Int32 sortValue1 = RecordTypeToSortValue[record1.RecordType];
            Int32 sortValue2 = RecordTypeToSortValue[record2.RecordType];
            return sortValue1.CompareTo(sortValue2);
         });

         return allNvpRecords;
      }

      #endregion

      #region Utility Methods

      /// <summary>
      /// Updates the current status message, the resources loaded, and the number of resources to load.
      /// </summary>
      private void ReportInitializationDataProgress(String message, Int32 resourcesLoaded, Int32 resourcesToLoad)
      {
         CurrentStatusMessage = message;
         ResourcesLoaded = resourcesLoaded;
         ResourcesToLoad = resourcesToLoad;
      }

      /// <summary>
      /// Simulates a delay.  This is useful to test scaleability.
      /// </summary>
      private void SimulateLoadingDelay()
      {
         if (SimulatedLoadingDelay > 0)
         {
            Thread.Sleep(SimulatedLoadingDelay);
         }
      }

      #endregion

      #endregion

   }
}