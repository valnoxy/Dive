#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Dive.Core.Action.DiskPreparation
{
    /// <summary>
    /// Forwards the log output of the native DiskMgr library to managed code.
    /// As soon as someone subscribes to <see cref="MessageLogged"/>, DiskMgr no longer
    /// writes to the console but hands every message to this class.
    /// </summary>
    public static class DiskMgrLog
    {
        public enum Level
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private const string DllPath = "DiskMgr.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void LogCallback(int level, IntPtr message);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DiskMgr_SetLogCallback(LogCallback? callback);

        // Must stay referenced for the lifetime of the process, otherwise the GC
        // collects the delegate while native code still calls it.
        private static readonly LogCallback Callback = OnNativeLog;
        private static readonly object SyncRoot = new();
        private static Action<Level, string>? _handlers;
        private static bool _registered;

        public static event Action<Level, string> MessageLogged
        {
            add
            {
                lock (SyncRoot)
                {
                    _handlers += value;
                    if (_registered) return;
                    try
                    {
                        DiskMgr_SetLogCallback(Callback);
                        _registered = true;
                    }
                    catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
                    {
                        // DiskMgr not available (yet) - keep its console output.
                    }
                }
            }
            remove
            {
                lock (SyncRoot) _handlers -= value;
            }
        }

        private static void OnNativeLog(int level, IntPtr message)
        {
            var text = Marshal.PtrToStringUni(message) ?? string.Empty;
            var lvl = Enum.IsDefined(typeof(Level), level) ? (Level)level : Level.Info;

            Action<Level, string>? handlers;
            lock (SyncRoot) handlers = _handlers;

            try
            {
                handlers?.Invoke(lvl, text);
            }
            catch
            {
                // Never let a managed exception unwind into native code.
            }
        }
    }
}
