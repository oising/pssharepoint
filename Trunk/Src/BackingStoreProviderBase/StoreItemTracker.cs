#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Nivot.PowerShell
{
    internal static class StoreItemTracker
    {
        internal const int OP_CTOR = 0;
        internal const int OP_DISPOSE = 1;
        internal const int OP_FINALIZE = 2;

        private static Dictionary<string, long[]> s_tracker;

        static StoreItemTracker()
        {
            s_tracker = new Dictionary<string, long[]>();
        }

        private static void EnsureKey(string name)
        {
            if (s_tracker.ContainsKey(name))
            {
                return;
            }
            s_tracker.Add(name, new long[3] {0,0,0});
        }

        [Conditional("DEBUG")]
        internal static void IncrNewObject(string name)
        {
            lock (typeof(StoreItemTracker))
            {
                EnsureKey(name);
                s_tracker[name][OP_CTOR]++;
            }
        }

        [Conditional("DEBUG")]
        internal static void IncrDispose(string name)
        {
            lock (typeof(StoreItemTracker))
            {
                EnsureKey(name);
                s_tracker[name][OP_DISPOSE]++;
            }
        }

        [Conditional("DEBUG")]
        internal static void IncrFinalize(string name)
        {
            lock (typeof(StoreItemTracker))
            {
                EnsureKey(name);
                s_tracker[name][OP_FINALIZE]++;
            }
        }

        internal static Dictionary<string, long[]> GetTrackerData()
        {
            lock (typeof(StoreItemTracker))
            {
                // make a copy
                Dictionary<string, long[]> data = new Dictionary<string, long[]>(s_tracker);
                return data;
            }

        }

        internal static void Reset()
        {
            lock(typeof(StoreItemTracker))
            {
                // create new table
                s_tracker = new Dictionary<string, long[]>();
            }
        }
    }
}
