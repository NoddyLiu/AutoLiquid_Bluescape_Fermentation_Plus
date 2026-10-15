using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
   public  class WindowUtils
    {
        internal const int SWP_NOSIZE = 0x0001;
        internal const int SWP_NOMOVE = 0x0002;
        internal const int SWP_NOZORDER = 0x0004;
        internal const int SWP_FRAMECHANGED = 0x0020;
        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_DLGMODALFRAME = 0x0001;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        /// <summary>
        /// Hides icon for window.
        /// If this is called before InitializeComponent() then the icon will be completely removed from the title bar
        /// If this is called after InitializeComponent() then an empty image is used but there will be empty space between window border and title
        /// </summary>
        /// <param name="window">Window class</param>
        public static void HideIcon(System.Windows.Window window)
        {
            if (window.IsInitialized)
            {
                window.Icon = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
            }
            else
            {
                window.SourceInitialized += delegate
                {
                    // Get this window's handle
                    var hwnd = new WindowInteropHelper(window).Handle;

                    // Change the extended window style to not show a window icon
                    int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

                    // Update the window's non-client area to reflect the changes
                    SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                };
            }
        }
    }
}
