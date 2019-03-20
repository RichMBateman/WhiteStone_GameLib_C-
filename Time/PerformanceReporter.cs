using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteStone.GameLib.Time
{
   public static class PerformanceReporter
   {
      #region Private Static Members

      /// <summary>
      /// Mapping of stopwatch key to an actual stopwatch
      /// </summary>
      private static readonly Dictionary<String, Stopwatch> m_stopwatchCollection = new Dictionary<String, Stopwatch>();
      /// <summary>
      /// Mapping of a stopwatch key to the number of times this block of code has been called.
      /// </summary>
      private static readonly Dictionary<String, Int32> m_stopwatchKeyToNumCalls = new Dictionary<String, Int32>();

      #endregion

      #region Public Static Properties

      /// <summary>
      /// Controls whether calls to the performance api actually does anything.
      /// </summary>
      public static Boolean EnablePerformanceTiming = false;

      #endregion

      #region Public Methods

      /// <summary>
      /// Starts the stopwatch  for this key. (if performance timing is enabled).
      /// </summary>
      public static void StopwatchStart(String stopwatchKey)
      {
         if (EnablePerformanceTiming)
         {
            Stopwatch stopwatch = GetAddStopwatchByKey(stopwatchKey);
            stopwatch.Start();
         }
      }

      /// <summary>
      /// Stops the stopwatch, and resets it by default.
      /// </summary>
      public static void StopwatchStop(String stopwatchKey)
      {
         if (EnablePerformanceTiming)
         {
            Stopwatch stopwatch = GetAddStopwatchByKey(stopwatchKey);
            stopwatch.Stop();

            // Before any possible reporting, adjust the number of calls
            m_stopwatchKeyToNumCalls[stopwatchKey]++;
         }
      }

      /// <summary>
      /// Resets all stop watches.
      /// </summary>
      public static void StopwatchReset()
      {
         if (EnablePerformanceTiming)
         {
            foreach (String stopwatchKey in m_stopwatchCollection.Keys)
            {
               StopwatchReset(stopwatchKey);
            }
         }
      }

      /// <summary>
      /// Resets a stop watch.  Resets the number of recorded calls to 0, and resets the timer.
      /// </summary>
      public static void StopwatchReset(String stopwatchKey)
      {
         if (EnablePerformanceTiming)
         {
            Stopwatch stopwatch = GetAddStopwatchByKey(stopwatchKey);
            StopwatchReset(stopwatchKey, stopwatch);
         }
      }

      /// <summary>
      /// Returns information about all stopwatches.
      /// </summary>
      public static String StopwatchReport()
      {
         String report = String.Empty;
         if (EnablePerformanceTiming)
         {
            report = "<<<Performance Report>>>" + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            foreach (String stopwatchKey in m_stopwatchCollection.Keys)
            {
               String indReport = StopwatchReport(stopwatchKey);
               report = String.Concat(report, Environment.NewLine, Environment.NewLine, indReport);
            }
         }
         return report;
      }


      /// <summary>
      /// Outputs to the console information about this stopwatch.
      /// </summary>
      public static String StopwatchReport(String stopwatchKey)
      {
         String report = String.Empty;
         if (EnablePerformanceTiming)
         {
            Stopwatch stopwatch = GetAddStopwatchByKey(stopwatchKey);
            report = StopwatchReport(stopwatch, stopwatchKey);
         }
         return report;
      }

      #endregion

      #region Private Functions

      /// <summary>
      /// Completely resets a stopwatch.  Sets number of calls to 0, and resets the stopwatch.
      /// </summary>
      private static void StopwatchReset(String stopwatchKey, Stopwatch stopwatch)
      {
         m_stopwatchKeyToNumCalls[stopwatchKey] = 0;
         stopwatch.Reset();
      }

      /// <summary>
      /// Returns a stopwatch for the given key.  Creates a new one if necessary.
      /// </summary>
      private static Stopwatch GetAddStopwatchByKey(String key)
      {
         Stopwatch stopwatchForKey;
         if (!m_stopwatchCollection.ContainsKey(key))
         {
            stopwatchForKey = new Stopwatch();
            m_stopwatchCollection.Add(key, stopwatchForKey);
            m_stopwatchKeyToNumCalls.Add(key, 0);
         }
         else
         {
            stopwatchForKey = m_stopwatchCollection[key];
         }
         return stopwatchForKey;
      }

      /// <summary>
      /// For a given stopwatch Key and a stopwatch, returns a String which reports on the performance of that stopwatch.
      /// </summary>
      private static String StopwatchReport(Stopwatch stopwatch, String stopwatchKey)
      {
         Int32 numCalls = m_stopwatchKeyToNumCalls[stopwatchKey];
         Int64 elapsedMs = stopwatch.ElapsedMilliseconds;
         String report = String.Concat("<", stopwatchKey, ">", Environment.NewLine, "\t",
            "Num Calls: ", numCalls, Environment.NewLine,
            "\t" + "Elapsed Time (ms): ", elapsedMs, Environment.NewLine);

         // If only one call was made, there's no point reporting an average.
         if (numCalls > 1)
         {
            Double averageTime = (((Double)stopwatch.ElapsedMilliseconds) / numCalls);
            String averageTimePortion = String.Format("{0:f4}", averageTime);
            report = String.Concat(report, "\tAverage Time (ms): ", averageTimePortion, Environment.NewLine);
         }

         return report;

      }
      #endregion

   }
}
