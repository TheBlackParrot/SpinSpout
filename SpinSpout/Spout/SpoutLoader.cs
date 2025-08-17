// ReeCamera - Spout2 camera system for Beat Saber
// https://github.com/Reezonate/ReeCamera

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;

namespace SpinSpout.Spout;

internal static class SpoutLoader {
    private static readonly string DllPath = Path.Combine(Paths.GameRootPath, @"SpinRhythm_Data\Plugins\x86_64\KlakSpout.dll");
    private const string RESOURCE_NAME = nameof(SpinSpout) + ".KlakSpout.dll";

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    public static void LoadPlugin() {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME)) {
            using (FileStream fs = new(DllPath, FileMode.Create, FileAccess.Write)) {
                stream?.CopyTo(fs);
            }
        }

        IntPtr handle = LoadLibrary(DllPath);
        if (handle == IntPtr.Zero) {
            int errorCode = Marshal.GetLastWin32Error();
            Plugin.Logger.LogError($"Failed to load Spout DLL! Win32 Error Code: {errorCode}");
        } else {
            Plugin.Logger.LogMessage("Spout loaded Successfully!");
        }
    }
}