﻿#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * Contributions from Erik Ylvisaker
 * See license.txt for license info
 */
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

/* TODO: Update the description of TimeBeginPeriod and other native methods. Update Timer. */

#pragma warning disable 3019    // CLS-compliance checking
#pragma warning disable 0649    // struct members not explicitly initialized
#pragma warning disable 0169    // field / method is never used.
#pragma warning disable 0414    // field assigned but never used.

namespace OpenTK.Platform.Windows
{
    #region Type aliases

    using HWND = System.IntPtr;
    using HINSTANCE = System.IntPtr;
    using HMENU = System.IntPtr;
    using HICON = System.IntPtr;
    using HBRUSH = System.IntPtr;
    using HCURSOR = System.IntPtr;

    using LRESULT = System.IntPtr;
    using LPVOID = System.IntPtr;
    using LPCTSTR = System.String;

    using WPARAM = System.IntPtr;
    using LPARAM = System.IntPtr;
    using HANDLE = System.IntPtr;
    using HRAWINPUT = System.IntPtr;

    using BYTE = System.Byte;
    using SHORT = System.Int16;
    using USHORT = System.UInt16;
    using LONG = System.Int32;
    using ULONG = System.UInt32;
    using WORD = System.Int16;
    using DWORD = System.Int32;
    using BOOL = System.Boolean;
    using INT = System.Int32;
    using UINT = System.UInt32;
    using LONG_PTR = System.IntPtr;
    using ATOM = System.Int32;

    using COLORREF = System.Int32;
    using RECT = OpenTK.Platform.Windows.Win32Rectangle;
    using WNDPROC = System.IntPtr;
    using LPDEVMODE = DeviceMode;

    using HRESULT = System.IntPtr;
    using HMONITOR = System.IntPtr;

    using DWORD_PTR = System.IntPtr;
    using UINT_PTR = System.UIntPtr;

    #endregion

    /// \internal
    /// <summary>
    /// For internal use by OpenTK only!
    /// Exposes useful native WINAPI methods and structures.
    /// </summary>
    internal static class Functions
    {
        #region Window functions

        #region SetWindowPos

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(
            IntPtr handle,
            IntPtr insertAfter,
            int x, int y, int cx, int cy,
            SetWindowPosFlags flags
        );

        #endregion

        #region AdjustWindowRect

        /// <summary>
        /// Calculates the required size of the window rectangle, based on the desired client-rectangle size. The window rectangle can then be passed to the CreateWindow function to create a window whose client area is the desired size.
        /// </summary>
        /// <param name="lpRect">[in, out] Pointer to a RECT structure that contains the coordinates of the top-left and bottom-right corners of the desired client area. When the function returns, the structure contains the coordinates of the top-left and bottom-right corners of the window to accommodate the desired client area.</param>
        /// <param name="dwStyle">[in] Specifies the window style of the window whose required size is to be calculated. Note that you cannot specify the WS_OVERLAPPED style.</param>
        /// <param name="bMenu">[in] Specifies whether the window has a menu.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        /// A client rectangle is the smallest rectangle that completely encloses a client area. A window rectangle is the smallest rectangle that completely encloses the window, which includes the client area and the nonclient area. 
        /// The AdjustWindowRect function does not add extra space when a menu bar wraps to two or more rows. 
        /// The AdjustWindowRect function does not take the WS_VSCROLL or WS_HSCROLL styles into account. To account for the scroll bars, call the GetSystemMetrics function with SM_CXVSCROLL or SM_CYHSCROLL.
        /// Found Winuser.h, user32.dll
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern BOOL AdjustWindowRect([In, Out] ref Win32Rectangle lpRect, WindowStyle dwStyle, BOOL bMenu);

        [DllImport("user32.dll", EntryPoint = "AdjustWindowRectEx", CallingConvention = CallingConvention.StdCall, SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool AdjustWindowRectEx(ref Win32Rectangle lpRect, WindowStyle dwStyle, bool bMenu, ExtendedWindowStyle dwExStyle);

        #endregion

        #region CreateWindowEx

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(
            ExtendedWindowStyle ExStyle,
            IntPtr ClassAtom,
            IntPtr WindowName,
            WindowStyle Style,
            int X, int Y,
            int Width, int Height,
            IntPtr HandleToParentWindow,
            IntPtr Menu,
            IntPtr Instance,
            IntPtr Param);

        #region DestroyWindow

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(IntPtr windowHandle);

        #endregion

        #region RegisterClassEx

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern ushort RegisterClassEx(ref ExtendedWindowClass window_class);

        #endregion

        #region UnregisterClass

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern short UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR className, IntPtr instance);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern short UnregisterClass(IntPtr className, IntPtr instance);

        #endregion

        #region SetWindowLong

        // SetWindowLongPtr does not exist on x86 platforms (it's a macro that resolves to SetWindowLong).
        // We need to detect if we are on x86 or x64 at runtime and call the correct function
        // (SetWindowLongPtr on x64 or SetWindowLong on x86). Fun!
        internal static IntPtr SetWindowLong(IntPtr handle, GetWindowLongOffsets item, IntPtr newValue)
        {
            // SetWindowPos defines its error condition as an IntPtr.Zero retval and a non-0 GetLastError.
            // We need to SetLastError(0) to ensure we are not detecting on older error condition (from another function).

            IntPtr retval = IntPtr.Zero;
            SetLastError(0);

            if (IntPtr.Size == 4)
                retval = new IntPtr(SetWindowLong(handle, item, newValue.ToInt32()));
            else
                retval = SetWindowLongPtr(handle, item, newValue);

            if (retval == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new PlatformException(String.Format("Failed to modify window border. Error: {0}", error));
            }

            return retval;
        }

        internal static IntPtr SetWindowLong(IntPtr handle, WindowProcedure newValue)
        {
            return SetWindowLong(handle, GetWindowLongOffsets.WNDPROC, Marshal.GetFunctionPointerForDelegate(newValue));
        }

#if RELASE
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport("user32.dll", SetLastError = true)]
        static extern LONG SetWindowLong(HWND hWnd, GetWindowLongOffsets nIndex, LONG dwNewLong);

#if RELASE
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport("user32.dll", SetLastError = true)]
        static extern LONG_PTR SetWindowLongPtr(HWND hWnd, GetWindowLongOffsets nIndex, LONG_PTR dwNewLong);

#if RELASE
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport("user32.dll", SetLastError = true)]
        static extern LONG SetWindowLong(HWND hWnd, GetWindowLongOffsets nIndex,
            [MarshalAs(UnmanagedType.FunctionPtr)]WindowProcedure dwNewLong);

#if RELASE
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport("user32.dll", SetLastError = true)]
        static extern LONG_PTR SetWindowLongPtr(HWND hWnd, GetWindowLongOffsets nIndex,
            [MarshalAs(UnmanagedType.FunctionPtr)]WindowProcedure dwNewLong);

        #endregion

        #region GetWindowLong

        internal static UIntPtr GetWindowLong(IntPtr handle, GetWindowLongOffsets index)
        {
            if (IntPtr.Size == 4)
                return (UIntPtr)GetWindowLongInternal(handle, index);

            return GetWindowLongPtrInternal(handle, index);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true, EntryPoint="GetWindowLong")]
        static extern ULONG GetWindowLongInternal(HWND hWnd, GetWindowLongOffsets nIndex);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtr")]
        static extern UIntPtr GetWindowLongPtrInternal(HWND hWnd, GetWindowLongOffsets nIndex);

        #endregion

        #endregion

        #region Message handling

        #region PeekMessage

        /// <summary>
        /// Low-level WINAPI function that checks the next message in the queue.
        /// </summary>
        /// <param name="msg">The pending message (if any) is stored here.</param>
        /// <param name="hWnd">Not used</param>
        /// <param name="messageFilterMin">Not used</param>
        /// <param name="messageFilterMax">Not used</param>
        /// <param name="flags">Not used</param>
        /// <returns>True if there is a message pending.</returns>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessage(ref MSG msg, IntPtr hWnd, int messageFilterMin, int messageFilterMax, int flags);

        #endregion

        #region GetMessage

        /// <summary>
        /// Low-level WINAPI function that retriives the next message in the queue.
        /// </summary>
        /// <param name="msg">The pending message (if any) is stored here.</param>
        /// <param name="windowHandle">Not used</param>
        /// <param name="messageFilterMin">Not used</param>
        /// <param name="messageFilterMax">Not used</param>
        /// <returns>
        /// Nonzero indicates that the function retrieves a message other than WM_QUIT.
        /// Zero indicates that the function retrieves the WM_QUIT message, or that lpMsg is an invalid pointer.
        /// 1 indicates that an error occurred  for example, the function fails if hWnd is an invalid window handle.
        /// To get extended error information, call GetLastError.
        /// </returns>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        internal static extern INT GetMessage(ref MSG msg,
            IntPtr windowHandle, int messageFilterMin, int messageFilterMax);

        #endregion

        #region SendMessage

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern LRESULT SendMessage(HWND hWnd, WindowMessage Msg, WPARAM wParam, LPARAM lParam);

        #endregion

        #region PostMessage

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern BOOL PostMessage(HWND hWnd, WindowMessage Msg, WPARAM wParam, LPARAM lParam);

        #endregion

        #region DispatchMessage

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll")]
        internal static extern LRESULT DispatchMessage(ref MSG msg);

        #endregion

        #region TranslateMessage

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll")]
        internal static extern BOOL TranslateMessage(ref MSG lpMsg);

        #endregion

        #region DefWindowProc

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr DefWindowProc(HWND hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #endregion

        #region Rendering

        #region GetDC

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        #endregion

        #region ReleaseDC

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr hwnd, IntPtr DC);

        #endregion

        #region ChoosePixelFormat

        [DllImport("gdi32.dll")]
        internal static extern int ChoosePixelFormat(IntPtr dc, ref PixelFormatDescriptor pfd);

        #endregion

        [DllImport("gdi32.dll")]
        internal static extern int DescribePixelFormat(IntPtr deviceContext, int pixel, int pfdSize, ref PixelFormatDescriptor pixelFormat);

        #region SetPixelFormat

        [DllImport("gdi32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetPixelFormat(IntPtr dc, int format, ref PixelFormatDescriptor pfd);

        #endregion

        #region SwapBuffers

        [SuppressUnmanagedCodeSecurity]
        [DllImport("gdi32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SwapBuffers(IntPtr dc);

        #endregion

        #endregion

        #region DLL handling

        [DllImport("kernel32.dll")]
        internal static extern void SetLastError(DWORD dwErrCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        #endregion

        #region MapVirtualKey

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern UINT MapVirtualKey(VirtualKeys vkey, MapVirtualKeyType uMapType);

        #endregion

        #region ShowWindow

        /// <summary>
        /// The ShowWindow function sets the specified window's show state.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window.</param>
        /// <param name="nCmdShow">[in] Specifies how the window is to be shown. This parameter is ignored the first time an application calls ShowWindow, if the program that launched the application provides a STARTUPINFO structure. Otherwise, the first time ShowWindow is called, the value should be the value obtained by the WinMain function in its nCmdShow parameter. In subsequent calls, this parameter can be one of the ShowWindowEnum values.</param>
        /// <returns>If the window was previously visible, the return value is true. Otherwise false.</returns>
        /// <remarks>
        /// <para>To perform certain special effects when showing or hiding a window, use AnimateWindow.</para>
        /// <para>The first time an application calls ShowWindow, it should use the WinMain function's nCmdShow parameter as its nCmdShow parameter. Subsequent calls to ShowWindow must use one of the values in the given list, instead of the one specified by the WinMain function's nCmdShow parameter.</para>
        /// <para>As noted in the discussion of the nCmdShow parameter, the nCmdShow value is ignored in the first call to ShowWindow if the program that launched the application specifies startup information in the structure. In this case, ShowWindow uses the information specified in the STARTUPINFO structure to show the window. On subsequent calls, the application must call ShowWindow with nCmdShow set to SW_SHOWDEFAULT to use the startup information provided by the program that launched the application. This behavior is designed for the following situations:</para>
        /// <list type="">
        /// <item>Applications create their main window by calling CreateWindow with the WS_VISIBLE flag set.</item>
        /// <item>Applications create their main window by calling CreateWindow with the WS_VISIBLE flag cleared, and later call ShowWindow with the SW_SHOW flag set to make it visible.</item>
        /// </list>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern BOOL ShowWindow(HWND hWnd, ShowWindowCommand nCmdShow);

        #endregion

        #region SetWindowText

        /// <summary>
        /// The SetWindowText function changes the text of the specified window's title bar (if it has one). If the specified window is a control, the text of the control is changed. However, SetWindowText cannot change the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window or control whose text is to be changed.</param>
        /// <param name="lpString">[in] Pointer to a null-terminated string to be used as the new title or control text.</param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </returns>
        /// <remarks>
        /// <para>If the target window is owned by the current process, SetWindowText causes a WM_SETTEXT message to be sent to the specified window or control. If the control is a list box control created with the WS_CAPTION style, however, SetWindowText sets the text for the control, not for the list box entries. </para>
        /// <para>To set the text of a control in another process, send the WM_SETTEXT message directly instead of calling SetWindowText. </para>
        /// <para>The SetWindowText function does not expand tab characters (ASCII code 0x09). Tab characters are displayed as vertical bar (|) characters. </para>
        /// <para>Windows 95/98/Me: SetWindowTextW is supported by the Microsoft Layer for Unicode (MSLU). To use this, you must add certain files to your application, as outlined in Microsoft Layer for Unicode on Windows 95/98/Me Systems .</para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern BOOL SetWindowText(HWND hWnd, [MarshalAs(UnmanagedType.LPTStr)] string lpString);

        #endregion

        #region GetWindowText

        /// <summary>
        /// The GetWindowText function copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">[in] Handle to the window or control containing the text.</param>
        /// <param name="lpString">[out] Pointer to the buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a NULL character.</param>
        /// <param name="nMaxCount">[in] Specifies the maximum number of characters to copy to the buffer, including the NULL character. If the text exceeds this limit, it is truncated.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the copied string, not including the terminating NULL character. If the window has no title bar or text, if the title bar is empty, or if the window or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.
        /// <para>This function cannot retrieve the text of an edit control in another application.</para>
        /// </returns>
        /// <remarks>
        /// <para>If the target window is owned by the current process, GetWindowText causes a WM_GETTEXT message to be sent to the specified window or control. If the target window is owned by another process and has a caption, GetWindowText retrieves the window caption text. If the window does not have a caption, the return value is a null string. This behavior is by design. It allows applications to call GetWindowText without becoming unresponsive if the process that owns the target window is not responding. However, if the target window is not responding and it belongs to the calling application, GetWindowText will cause the calling application to become unresponsive.</para>
        /// <para>To retrieve the text of a control in another process, send a WM_GETTEXT message directly instead of calling GetWindowText.</para>
        /// <para>Windows 95/98/Me: GetWindowTextW is supported by the Microsoft Layer for Unicode (MSLU). To use this, you must add certain files to your application, as outlined in Microsoft Layer for Unicode on Windows 95/98/Me</para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(HWND hWnd, [MarshalAs(UnmanagedType.LPTStr), In, Out] StringBuilder lpString, int nMaxCount);

        #endregion

        #region ScreenToClient

        /// <summary>
        /// Converts the screen coordinates of a specified point on the screen to client-area coordinates.
        /// </summary>
        /// <param name="hWnd">Handle to the window whose client area will be used for the conversion.</param>
        /// <param name="point">Pointer to a POINT structure that specifies the screen coordinates to be converted.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. Windows NT/2000/XP: To get extended error information, call GetLastError.</returns>
        /// <remarks>
        /// <para>The function uses the window identified by the hWnd parameter and the screen coordinates given in the POINT structure to compute client coordinates. It then replaces the screen coordinates with the client coordinates. The new coordinates are relative to the upper-left corner of the specified window's client area. </para>
        /// <para>The ScreenToClient function assumes the specified point is in screen coordinates. </para>
        /// <para>All coordinates are in device units.</para>
        /// <para>Do not use ScreenToClient when in a mirroring situation, that is, when changing from left-to-right layout to right-to-left layout. Instead, use MapWindowPoints. For more information, see "Window Layout and Mirroring" in Window Features.</para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        //internal static extern BOOL ScreenToClient(HWND hWnd, ref POINT point);
        internal static extern BOOL ScreenToClient(HWND hWnd, ref Point point);

        #endregion

        #region GetClientRect

        /// <summary>
        /// The GetClientRect function retrieves the coordinates of a window's client area. The client coordinates specify the upper-left and lower-right corners of the client area. Because client coordinates are relative to the upper-left corner of a window's client area, the coordinates of the upper-left corner are (0,0).
        /// </summary>
        /// <param name="windowHandle">Handle to the window whose client coordinates are to be retrieved.</param>
        /// <param name="clientRectangle">Pointer to a RECT structure that receives the client coordinates. The left and top members are zero. The right and bottom members contain the width and height of the window.</param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para>
        /// </returns>
        /// <remarks>In conformance with conventions for the RECT structure, the bottom-right coordinates of the returned rectangle are exclusive. In other words, the pixel at (right, bottom) lies immediately outside the rectangle.</remarks>
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal extern static BOOL GetClientRect(HWND windowHandle, out Win32Rectangle clientRectangle);

        #endregion

        #region IsWindowVisible

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr intPtr);

        #endregion

        #region LoadCursor

        [DllImport("user32.dll")]
        public static extern HCURSOR LoadCursor(HINSTANCE hInstance, LPCTSTR lpCursorName);

        [DllImport("user32.dll")]
        public static extern HCURSOR LoadCursor(HINSTANCE hInstance, IntPtr lpCursorName);

        public static HCURSOR LoadCursor(CursorName lpCursorName)
        {
            return LoadCursor(IntPtr.Zero, new IntPtr((int)lpCursorName));
        }

        #endregion

        [DllImport("user32.dll", SetLastError=true)]
        public static extern BOOL SetForegroundWindow(HWND hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern BOOL BringWindowToTop(HWND hWnd);

        #endregion

        #region Display settings

        #region ChangeDisplaySettingsEx

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern LONG ChangeDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpszDeviceName,
            LPDEVMODE lpDevMode, HWND hwnd, ChangeDisplaySettingsEnum dwflags, LPVOID lParam);

        #endregion

        #region EnumDisplayDevices

        [DllImport("user32.dll", SetLastError = true, CharSet=CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern BOOL EnumDisplayDevices([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpDevice,
            DWORD iDevNum, [In, Out] WindowsDisplayDevice lpDisplayDevice, DWORD dwFlags);

        #endregion

        #region EnumDisplaySettings

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern BOOL EnumDisplaySettings([MarshalAs(UnmanagedType.LPTStr)] string device_name,
            int graphics_mode, [In, Out] DeviceMode device_mode);

        #endregion

        #region EnumDisplaySettingsEx

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern BOOL EnumDisplaySettingsEx([MarshalAs(UnmanagedType.LPTStr)] LPCTSTR lpszDeviceName, DisplayModeSettingsEnum iModeNum,
            [In, Out] DeviceMode lpDevMode, DWORD dwFlags);

        #endregion

        #region GetMonitorInfo

        [DllImport("user32.dll", SetLastError = true)]
        public static extern BOOL GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        #endregion

        #region MonitorFromWindow

        [DllImport("user32.dll", SetLastError = true)]
        public static extern HMONITOR MonitorFromWindow(HWND hwnd, MonitorFrom dwFlags);

        #endregion

        #endregion

        #region Input functions

        [DllImport("user32.dll", SetLastError=true)]
        public static extern BOOL TrackMouseEvent(ref TrackMouseEventStructure lpEventTrack);

        #region Async input

        #region GetCursorPos

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <param name="point">Pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns nonzero if successful or zero otherwise. To get extended error information, call GetLastError.</returns>
        /// <remarks>
        /// <para>The cursor position is always specified in screen coordinates and is not affected by the mapping mode of the window that contains the cursor.</para>
        /// <para>The calling process must have WINSTA_READATTRIBUTES access to the window station.</para>
        /// <para>The input desktop must be the current desktop when you call GetCursorPos. Call OpenInputDesktop to determine whether the current desktop is the input desktop. If it is not, call SetThreadDesktop with the HDESK returned by OpenInputDesktop to switch to that desktop.</para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern BOOL GetCursorPos(ref Point point);

        #endregion
        
        #region SetCursorPos
        
        [DllImport("user32.dll", SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern BOOL SetCursorPos(int x, int y);
        
        #endregion

        #endregion

        #endregion
    }

    #region --- Constants ---

        internal struct Constants
        {
            // Device mode types (found in wingdi.h)
            internal const int DM_BITSPERPEL = 0x00040000;
            internal const int DM_PELSWIDTH = 0x00080000;
            internal const int DM_PELSHEIGHT = 0x00100000;
            internal const int DM_DISPLAYFLAGS = 0x00200000;
            internal const int DM_DISPLAYFREQUENCY = 0x00400000;

            // ChangeDisplaySettings results (found in winuser.h)
            internal const int DISP_CHANGE_SUCCESSFUL = 0;
            internal const int DISP_CHANGE_RESTART = 1;
            internal const int DISP_CHANGE_FAILED = -1;
        }

        #endregion

    #region --- Structures ---

    #region CreateStruct

    internal struct CreateStruct
    {
        /// <summary>
        /// Contains additional data which may be used to create the window.
        /// </summary>
        /// <remarks>
        ///  If the window is being created as a result of a call to the CreateWindow
        ///  or CreateWindowEx function, this member contains the value of the lpParam 
        ///  parameter specified in the function call.
        ///  <para>
        /// If the window being created is a multiple-document interface (MDI) client window,
        /// this member contains a pointer to a CLIENTCREATESTRUCT structure. If the window
        /// being created is a MDI child window, this member contains a pointer to an 
        /// MDICREATESTRUCT structure.
        ///  </para>
        /// <para>
        /// Windows NT/2000/XP: If the window is being created from a dialog template,
        /// this member is the address of a SHORT value that specifies the size, in bytes,
        /// of the window creation data. The value is immediately followed by the creation data.
        /// </para>
        /// <para>
        /// Windows NT/2000/XP: You should access the data represented by the lpCreateParams member
        /// using a pointer that has been declared using the UNALIGNED type, because the pointer
        /// may not be DWORD aligned.
        /// </para>
        /// </remarks>
        internal LPVOID lpCreateParams;
        /// <summary>
        /// Handle to the module that owns the new window.
        /// </summary>
        internal HINSTANCE hInstance;
        /// <summary>
        /// Handle to the menu to be used by the new window.
        /// </summary>
        internal HMENU hMenu;
        /// <summary>
        /// Handle to the parent window, if the window is a child window.
        /// If the window is owned, this member identifies the owner window.
        /// If the window is not a child or owned window, this member is NULL.
        /// </summary>
        internal HWND hwndParent;
        /// <summary>
        /// Specifies the height of the new window, in pixels.
        /// </summary>
        internal int cy;
        /// <summary>
        /// Specifies the width of the new window, in pixels.
        /// </summary>
        internal int cx;
        /// <summary>
        /// Specifies the y-coordinate of the upper left corner of the new window.
        /// If the new window is a child window, coordinates are relative to the parent window.
        /// Otherwise, the coordinates are relative to the screen origin.
        /// </summary>
        internal int y;
        /// <summary>
        /// Specifies the x-coordinate of the upper left corner of the new window.
        /// If the new window is a child window, coordinates are relative to the parent window.
        /// Otherwise, the coordinates are relative to the screen origin.
        /// </summary>
        internal int x;
        /// <summary>
        /// Specifies the style for the new window.
        /// </summary>
        internal LONG style;
        /// <summary>
        /// Pointer to a null-terminated string that specifies the name of the new window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        internal LPCTSTR lpszName;
        /// <summary>
        /// Either a pointer to a null-terminated string or an atom that specifies the class name
        /// of the new window.
        /// <remarks>
        /// Note  Because the lpszClass member can contain a pointer to a local (and thus inaccessable) atom,
        /// do not obtain the class name by using this member. Use the GetClassName function instead.
        /// </remarks>
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        internal LPCTSTR lpszClass;
        /// <summary>
        /// Specifies the extended window style for the new window.
        /// </summary>
        internal DWORD dwExStyle;
    }

    #endregion

    #region StyleStruct

    struct StyleStruct
    {
        public WindowStyle Old;
        public WindowStyle New;
    }

    #endregion

    #region PixelFormatDescriptor

    /// \internal
    /// <summary>
    /// Describes a pixel format. It is used when interfacing with the WINAPI to create a new Context.
    /// Found in WinGDI.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PixelFormatDescriptor
    {
        internal short Size;
        internal short Version;
        internal PixelFormatDescriptorFlags Flags;
        internal PixelType PixelType;
        internal byte ColorBits;
        internal byte RedBits;
        internal byte RedShift;
        internal byte GreenBits;
        internal byte GreenShift;
        internal byte BlueBits;
        internal byte BlueShift;
        internal byte AlphaBits;
        internal byte AlphaShift;
        internal byte AccumBits;
        internal byte AccumRedBits;
        internal byte AccumGreenBits;
        internal byte AccumBlueBits;
        internal byte AccumAlphaBits;
        internal byte DepthBits;
        internal byte StencilBits;
        internal byte AuxBuffers;
        internal byte LayerType;
        private byte Reserved;
        internal int LayerMask;
        internal int VisibleMask;
        internal int DamageMask;
        
        public const short DefaultVersion = 1;
        public const short DefaultSize = 40;
    }
    
    #endregion

    #region DeviceMode
   
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class DeviceMode
    {
        internal DeviceMode()
        {
            Size = (short)Marshal.SizeOf(this);
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string DeviceName;
        internal short SpecVersion;
        internal short DriverVersion;
        private short Size;
        internal short DriverExtra;
        internal int Fields;

        internal POINT Position;
        internal DWORD DisplayOrientation;
        internal DWORD DisplayFixedOutput;

        internal short Color;
        internal short Duplex;
        internal short YResolution;
        internal short TTOption;
        internal short Collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string FormName;
        internal short LogPixels;
        internal int BitsPerPel;
        internal int PelsWidth;
        internal int PelsHeight;
        internal int DisplayFlags;
        internal int DisplayFrequency;
        internal int ICMMethod;
        internal int ICMIntent;
        internal int MediaType;
        internal int DitherType;
        internal int Reserved1;
        internal int Reserved2;
        internal int PanningWidth;
        internal int PanningHeight;
    }

    #endregion DeviceMode class

    #region DisplayDevice

    /// \internal
    /// <summary>
    /// The DISPLAY_DEVICE structure receives information about the display device specified by the iDevNum parameter of the EnumDisplayDevices function.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class WindowsDisplayDevice
    {
        internal WindowsDisplayDevice()
        {
            size = (short)Marshal.SizeOf(this);
        }
        readonly DWORD size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceString;
        internal DisplayDeviceStateFlags StateFlags;    // DWORD
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string DeviceKey;
    }

    #endregion

    #region Window Handling

    #region WindowClass
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowClass
    {
        internal ClassStyle Style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        internal WindowProcedure WindowProcedure;
        internal int ClassExtraBytes;
        internal int WindowExtraBytes;
        //[MarshalAs(UnmanagedType.
        internal IntPtr Instance;
        internal IntPtr Icon;
        internal IntPtr Cursor;
        internal IntPtr BackgroundBrush;
        //[MarshalAs(UnmanagedType.LPStr)]
        internal IntPtr MenuName;
        [MarshalAs(UnmanagedType.LPTStr)]
        internal string ClassName;
        //internal string ClassName;

        internal static int SizeInBytes = Marshal.SizeOf(default(WindowClass));
    }
    #endregion

    #region ExtendedWindowClass

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct ExtendedWindowClass
    {
        public UINT Size;
        public ClassStyle Style;
        //public WNDPROC WndProc;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WindowProcedure WndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public HINSTANCE Instance;
        public HICON Icon;
        public HCURSOR Cursor;
        public HBRUSH Background;
        public IntPtr MenuName;
        public IntPtr ClassName;
        public HICON IconSm;

        public static uint SizeInBytes = (uint)Marshal.SizeOf(default(ExtendedWindowClass));
    }

    #endregion

    #region internal struct WindowPosition

    /// \internal
    /// <summary>
    /// The WindowPosition structure contains information about the size and position of a window.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowPosition
    {
        /// <summary>
        /// Handle to the window.
        /// </summary>
        internal HWND hwnd;
        /// <summary>
        /// Specifies the position of the window in Z order (front-to-back position).
        /// This member can be a handle to the window behind which this window is placed,
        /// or can be one of the special values listed with the SetWindowPos function.
        /// </summary>
        internal HWND hwndInsertAfter;
        /// <summary>
        /// Specifies the position of the left edge of the window.
        /// </summary>
        internal int x;
        /// <summary>
        /// Specifies the position of the top edge of the window.
        /// </summary>
        internal int y;
        /// <summary>
        /// Specifies the window width, in pixels.
        /// </summary>
        internal int cx;
        /// <summary>
        /// Specifies the window height, in pixels.
        /// </summary>
        internal int cy;
        /// <summary>
        /// Specifies the window position.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        internal SetWindowPosFlags flags;
    }

    #region internal enum SetWindowPosFlags

    [Flags]
    internal enum SetWindowPosFlags : int
    {
        /// <summary>
        /// Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        NOSIZE          = 0x0001,
        /// <summary>
        /// Retains the current position (ignores the x and y parameters).
        /// </summary>
        NOMOVE          = 0x0002,
        /// <summary>
        /// Retains the current Z order (ignores the hwndInsertAfter parameter).
        /// </summary>
        NOZORDER        = 0x0004,
        /// <summary>
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs.
        /// This applies to the client area, the nonclient area (including the title bar and scroll bars),
        /// and any part of the parent window uncovered as a result of the window being moved.
        /// When this flag is set, the application must explicitly invalidate or redraw any parts
        /// of the window and parent window that need redrawing.
        /// </summary>
        NOREDRAW        = 0x0008,
        /// <summary>
        /// Does not activate the window. If this flag is not set,
        /// the window is activated and moved to the top of either the topmost or non-topmost group
        /// (depending on the setting of the hwndInsertAfter member).
        /// </summary>
        NOACTIVATE      = 0x0010,
        /// <summary>
        /// Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed.
        /// If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        FRAMECHANGED    = 0x0020, /* The frame changed: send WM_NCCALCSIZE */
        /// <summary>
        /// Displays the window.
        /// </summary>
        SHOWWINDOW      = 0x0040,
        /// <summary>
        /// Hides the window.
        /// </summary>
        HIDEWINDOW      = 0x0080,
        /// <summary>
        /// Discards the entire contents of the client area. If this flag is not specified,
        /// the valid contents of the client area are saved and copied back into the client area 
        /// after the window is sized or repositioned.
        /// </summary>
        NOCOPYBITS      = 0x0100,
        /// <summary>
        /// Does not change the owner window's position in the Z order.
        /// </summary>
        NOOWNERZORDER   = 0x0200, /* Don't do owner Z ordering */
        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        NOSENDCHANGING  = 0x0400, /* Don't send WM_WINDOWPOSCHANGING */

        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        DRAWFRAME       = FRAMECHANGED,
        /// <summary>
        /// Same as the NOOWNERZORDER flag.
        /// </summary>
        NOREPOSITION    = NOOWNERZORDER,

        DEFERERASE      = 0x2000,
        ASYNCWINDOWPOS  = 0x4000
    }

    #endregion

    #endregion

    #endregion

    #region Rectangle

    /// \internal
    /// <summary>
    /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive. In other words, the pixel whose coordinates are (right, bottom) lies immediately outside of the the rectangle. For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including, the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Rectangle
    {
        internal Win32Rectangle(int width, int height)
        {
            left = top = 0;
            right = width;
            bottom = height;
        }

        /// <summary>
        /// Specifies the x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal LONG left;
        /// <summary>
        /// Specifies the y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal LONG top;
        /// <summary>
        /// Specifies the x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal LONG right;
        /// <summary>
        /// Specifies the y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal LONG bottom;

        internal int Width { get { return right - left; } }
        internal int Height { get { return bottom - top; } }

        public override string ToString()
        {
            return String.Format("({0},{1})-({2},{3})", left, top, right, bottom);
        }

        internal Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        internal static Win32Rectangle From(Rectangle value)
        {
            Win32Rectangle rect = new Win32Rectangle();
            rect.left = value.Left;
            rect.right = value.Right;
            rect.top = value.Top;
            rect.bottom = value.Bottom;
            return rect;
        }

        internal static Win32Rectangle From(Size value)
        {
            Win32Rectangle rect = new Win32Rectangle();
            rect.left = 0;
            rect.right = value.Width;
            rect.top = 0;
            rect.bottom = value.Height;
            return rect;
        }
    }

    #endregion

    #region WindowInfo

    /// \internal
    /// <summary>
    /// Contains window information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct WindowInfo
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public DWORD Size;
        /// <summary>
        /// Pointer to a RECT structure that specifies the coordinates of the window. 
        /// </summary>
        public RECT Window;
        /// <summary>
        /// Pointer to a RECT structure that specifies the coordinates of the client area. 
        /// </summary>
        public RECT Client;
        /// <summary>
        /// The window styles. For a table of window styles, see CreateWindowEx. 
        /// </summary>
        public WindowStyle Style;
        /// <summary>
        /// The extended window styles. For a table of extended window styles, see CreateWindowEx.
        /// </summary>
        public ExtendedWindowStyle ExStyle;
        /// <summary>
        /// The window status. If this member is WS_ACTIVECAPTION, the window is active. Otherwise, this member is zero.
        /// </summary>
        public DWORD WindowStatus;
        /// <summary>
        /// The width of the window border, in pixels. 
        /// </summary>
        public UINT WindowBordersX;
        /// <summary>
        /// The height of the window border, in pixels.
        /// </summary>
        public UINT WindowBordersY;
        /// <summary>
        /// The window class atom (see RegisterClass). 
        /// </summary>
        public ATOM WindowType;
        /// <summary>
        /// The Microsoft Windows version of the application that created the window. 
        /// </summary>
        public WORD CreatorVersion;
    }

    #endregion

    #region MonitorInfo

    struct MonitorInfo
    {
        public DWORD Size;
        public RECT Monitor;
        public RECT Work;
        public DWORD Flags;

        public static readonly int SizeInBytes = Marshal.SizeOf(default(MonitorInfo));
    }

    #endregion

    #region TrackMouseEventStructure

    struct TrackMouseEventStructure
    {
        public DWORD Size;
        public TrackMouseEventFlags Flags;
        public HWND TrackWindowHandle;
        public DWORD HoverTime;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(TrackMouseEventStructure));
    }

    #endregion

    #endregion

    #region --- Enums ---

    #region GetWindowLongOffset

    /// <summary>
    /// Window field offsets for GetWindowLong() and GetWindowLongPtr().
    /// </summary>
    enum GWL
    {
        WNDPROC = (-4),
        HINSTANCE = (-6),
        HWNDPARENT = (-8),
        STYLE = (-16),
        EXSTYLE = (-20),
        USERDATA = (-21),
        ID = (-12),
    }

    #endregion

    #region SizeMessage

    internal enum SizeMessage
    {
        MAXHIDE = 4,
        MAXIMIZED = 2,
        MAXSHOW = 3,
        MINIMIZED = 1,
        RESTORED = 0
    }

    #endregion

    #region internal enum DisplayModeSettingsEnum

    internal enum DisplayModeSettingsEnum
    {
        CurrentSettings = -1,
        RegistrySettings = -2
    }

    #endregion

    #region internal enum DisplayDeviceStateFlags

    [Flags]
    internal enum DisplayDeviceStateFlags
    {
        None              = 0x00000000,
        AttachedToDesktop = 0x00000001,
        MultiDriver       = 0x00000002,
        PrimaryDevice     = 0x00000004,
        MirroringDriver   = 0x00000008,
        VgaCompatible     = 0x00000010,
        Removable         = 0x00000020,
        ModesPruned       = 0x08000000,
        Remote            = 0x04000000,
        Disconnect        = 0x02000000,

        // Child device state
        Active            = 0x00000001,
        Attached          = 0x00000002,
    }

    #endregion

    #region internal enum ChangeDisplaySettingsEnum

    [Flags]
    internal enum ChangeDisplaySettingsEnum
    {
        // ChangeDisplaySettings types (found in winuser.h)
        UpdateRegistry = 0x00000001,
        Test = 0x00000002,
        Fullscreen = 0x00000004,
    }

    #endregion

    #region internal enum WindowStyle : uint

    [Flags]
    internal enum WindowStyle : uint
    {
        Overlapped = 0x00000000,
        Popup = 0x80000000,
        Child = 0x40000000,
        Minimize = 0x20000000,
        Visible = 0x10000000,
        Disabled = 0x08000000,
        ClipSiblings = 0x04000000,
        ClipChildren = 0x02000000,
        Maximize = 0x01000000,
        Caption = 0x00C00000,    // Border | DialogFrame
        Border = 0x00800000,
        DialogFrame = 0x00400000,
        VScroll = 0x00200000,
        HScreen = 0x00100000,
        SystemMenu = 0x00080000,
        ThickFrame = 0x00040000,
        Group = 0x00020000,
        TabStop = 0x00010000,

        MinimizeBox = 0x00020000,
        MaximizeBox = 0x00010000,

        Tiled = Overlapped,
        Iconic = Minimize,
        SizeBox = ThickFrame,
        TiledWindow = OverlappedWindow,

        // Common window styles:
        OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
        PopupWindow = Popup | Border | SystemMenu,
        ChildWindow = Child
    }

    #endregion

    #region internal enum ExtendedWindowStyle : uint

    [Flags]
    internal enum ExtendedWindowStyle : uint
    {
        DialogModalFrame = 0x00000001,
        NoParentNotify = 0x00000004,
        Topmost = 0x00000008,
        AcceptFiles = 0x00000010,
        Transparent = 0x00000020,

        // #if(WINVER >= 0x0400)
        MdiChild = 0x00000040,
        ToolWindow = 0x00000080,
        WindowEdge = 0x00000100,
        ClientEdge = 0x00000200,
        ContextHelp = 0x00000400,
        // #endif

        // #if(WINVER >= 0x0400)
        Right = 0x00001000,
        Left = 0x00000000,
        RightToLeftReading = 0x00002000,
        LeftToRightReading = 0x00000000,
        LeftScrollbar = 0x00004000,
        RightScrollbar = 0x00000000,

        ControlParent = 0x00010000,
        StaticEdge = 0x00020000,
        ApplicationWindow = 0x00040000,

        OverlappedWindow = WindowEdge | ClientEdge,
        PaletteWindow = WindowEdge | ToolWindow | Topmost,
        // #endif

        // #if(_WIN32_WINNT >= 0x0500)
        Layered = 0x00080000,
        // #endif

        // #if(WINVER >= 0x0500)
        NoInheritLayout = 0x00100000, // Disable inheritence of mirroring by children
        RightToLeftLayout = 0x00400000, // Right to left mirroring
        // #endif /* WINVER >= 0x0500 */

        // #if(_WIN32_WINNT >= 0x0501)
        Composited = 0x02000000,
        // #endif /* _WIN32_WINNT >= 0x0501 */

        // #if(_WIN32_WINNT >= 0x0500)
        NoActivate = 0x08000000
        // #endif /* _WIN32_WINNT >= 0x0500 */
    }

    #endregion

    #region GetWindowLongOffsets enum

    internal enum GetWindowLongOffsets : int
    {
        WNDPROC       = (-4),
        HINSTANCE     = (-6),
        HWNDPARENT    = (-8),
        STYLE         = (-16),
        EXSTYLE       = (-20),
        USERDATA      = (-21),
        ID            = (-12),
    }

    #endregion

    #region PixelFormatDescriptorFlags enum
    [Flags]
    internal enum PixelFormatDescriptorFlags : int
    {
        // PixelFormatDescriptor flags
        DOUBLEBUFFER = 0x01,
        STEREO = 0x02,
        DRAW_TO_WINDOW = 0x04,
        DRAW_TO_BITMAP = 0x08,
        SUPPORT_GDI = 0x10,
        SUPPORT_OPENGL = 0x20,
        GENERIC_FORMAT = 0x40,
        NEED_PALETTE = 0x80,
        NEED_SYSTEM_PALETTE = 0x100,
        SWAP_EXCHANGE = 0x200,
        SWAP_COPY = 0x400,
        SWAP_LAYER_BUFFERS = 0x800,
        GENERIC_ACCELERATED = 0x1000,
        SUPPORT_DIRECTDRAW = 0x2000,

        // PixelFormatDescriptor flags for use in ChoosePixelFormat only
        DEPTH_DONTCARE = unchecked((int)0x20000000),
        DOUBLEBUFFER_DONTCARE = unchecked((int)0x40000000),
        STEREO_DONTCARE = unchecked((int)0x80000000)
    }
    #endregion

    #region PixelType

    internal enum PixelType : byte
    {
        RGBA = 0,
        INDEXED = 1
    }

    #endregion

    #region WindowPlacementOptions enum

    internal enum WindowPlacementOptions
    {
        TOP = 0,
        BOTTOM = 1,
        TOPMOST = -1,
        NOTOPMOST = -2
    }

    #endregion

    #region ClassStyle enum
    [Flags]
    internal enum ClassStyle
    {
        //None            = 0x0000,
        VRedraw = 0x0001,
        HRedraw = 0x0002,
        DoubleClicks = 0x0008,
        OwnDC = 0x0020,
        ClassDC = 0x0040,
        ParentDC = 0x0080,
        NoClose = 0x0200,
        SaveBits = 0x0800,
        ByteAlignClient = 0x1000,
        ByteAlignWindow = 0x2000,
        GlobalClass = 0x4000,

        Ime = 0x00010000,

        // #if(_WIN32_WINNT >= 0x0501)
        DropShadow = 0x00020000
        // #endif /* _WIN32_WINNT >= 0x0501 */
    }
    #endregion

    #region RawInputDeviceFlags enum

    [Flags]
    internal enum RawInputDeviceFlags : int
    {
        /// <summary>
        /// If set, this removes the top level collection from the inclusion list.
        /// This tells the operating system to stop reading from a device which matches the top level collection.
        /// </summary>
        REMOVE          = 0x00000001,
        /// <summary>
        /// If set, this specifies the top level collections to exclude when reading a complete usage page.
        /// This flag only affects a TLC whose usage page is already specified with RawInputDeviceEnum.PAGEONLY. 
        /// </summary>
        EXCLUDE         = 0x00000010,
        /// <summary>
        /// If set, this specifies all devices whose top level collection is from the specified UsagePage.
        /// Note that usUsage must be zero. To exclude a particular top level collection, use EXCLUDE.
        /// </summary>
        PAGEONLY        = 0x00000020,
        /// <summary>
        /// If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages.
        /// This is only for the mouse and keyboard. See RawInputDevice Remarks.
        /// </summary>
        NOLEGACY        = 0x00000030,
        /// <summary>
        /// If set, this enables the caller to receive the input even when the caller is not in the foreground.
        /// Note that Target must be specified in RawInputDevice.
        /// </summary>
        INPUTSINK       = 0x00000100,
        /// <summary>
        /// If set, the mouse button click does not activate the other window.
        /// </summary>
        CAPTUREMOUSE    = 0x00000200, // effective when mouse nolegacy is specified, otherwise it would be an error
        /// <summary>
        /// If set, the application-defined keyboard device hotkeys are not handled.
        /// However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled.
        /// By default, all keyboard hotkeys are handled.
        /// NOHOTKEYS can be specified even if NOLEGACY is not specified and Target is NULL in RawInputDevice.
        /// </summary>
        NOHOTKEYS       = 0x00000200, // effective for keyboard.
        /// <summary>
        /// Microsoft Windows XP Service Pack 1 (SP1): If set, the application command keys are handled. APPKEYS can be specified only if NOLEGACY is specified for a keyboard device.
        /// </summary>
        APPKEYS         = 0x00000400, // effective for keyboard.
        /// <summary>
        /// If set, this enables the caller to receive input in the background only if the foreground application
        /// does not process it. In other words, if the foreground application is not registered for raw input,
        /// then the background application that is registered will receive the input.
        /// </summary>
        EXINPUTSINK     = 0x00001000,
        DEVNOTIFY       = 0x00002000,
        //EXMODEMASK      = 0x000000F0
    }

    #endregion

    #region GetRawInputDataEnum

    internal enum GetRawInputDataEnum
    {
        INPUT             = 0x10000003,
        HEADER            = 0x10000005
    }

    #endregion

    #region RawInputDeviceInfoEnum

    internal enum RawInputDeviceInfoEnum
    {
        PREPARSEDDATA    = 0x20000005,
        DEVICENAME       = 0x20000007,  // the return valus is the character length, not the byte size
        DEVICEINFO       = 0x2000000b
    }

    #endregion

    #region RawInputMouseState

    [Flags]
    internal enum RawInputMouseState : ushort
    {
        LEFT_BUTTON_DOWN = 0x0001,  // Left Button changed to down.
        LEFT_BUTTON_UP   = 0x0002,  // Left Button changed to up.
        RIGHT_BUTTON_DOWN   = 0x0004,  // Right Button changed to down.
        RIGHT_BUTTON_UP  = 0x0008,  // Right Button changed to up.
        MIDDLE_BUTTON_DOWN  = 0x0010,  // Middle Button changed to down.
        MIDDLE_BUTTON_UP = 0x0020,  // Middle Button changed to up.

        BUTTON_1_DOWN    = LEFT_BUTTON_DOWN,
        BUTTON_1_UP      = LEFT_BUTTON_UP,
        BUTTON_2_DOWN    = RIGHT_BUTTON_DOWN,
        BUTTON_2_UP      = RIGHT_BUTTON_UP,
        BUTTON_3_DOWN    = MIDDLE_BUTTON_DOWN,
        BUTTON_3_UP      = MIDDLE_BUTTON_UP,

        BUTTON_4_DOWN    = 0x0040,
        BUTTON_4_UP      = 0x0080,
        BUTTON_5_DOWN    = 0x0100,
        BUTTON_5_UP      = 0x0200,

        WHEEL            = 0x0400
    }

    #endregion

    #region RawInputKeyboardDataFlags

    internal enum RawInputKeyboardDataFlags : short //: ushort
    {
        MAKE            = 0,
        BREAK           = 1,
        E0              = 2,
        E1              = 4,
        TERMSRV_SET_LED = 8,
        TERMSRV_SHADOW  = 0x10
    }

    #endregion

    #region RawInputDeviceType

    internal enum RawInputDeviceType : int
    {
        MOUSE    = 0,
        KEYBOARD = 1,
        HID      = 2
    }

    #endregion

    #region RawMouseFlags

    /// <summary>
    /// Mouse indicator flags (found in winuser.h).
    /// </summary>
    internal enum RawMouseFlags : ushort
    {
        /// <summary>
        /// LastX/Y indicate relative motion.
        /// </summary>
        MOUSE_MOVE_RELATIVE = 0x00,
        /// <summary>
        /// LastX/Y indicate absolute motion.
        /// </summary>
        MOUSE_MOVE_ABSOLUTE = 0x01,
        /// <summary>
        /// The coordinates are mapped to the virtual desktop.
        /// </summary>
        MOUSE_VIRTUAL_DESKTOP = 0x02,
        /// <summary>
        /// Requery for mouse attributes.
        /// </summary>
        MOUSE_ATTRIBUTES_CHANGED = 0x04,
    }

    #endregion

    #region VirtualKeys

    internal enum VirtualKeys : short
    {
        /*
         * Virtual Key, Standard Set
         */
        LBUTTON      = 0x01,
        RBUTTON      = 0x02,
        CANCEL       = 0x03,
        MBUTTON      = 0x04,   /* NOT contiguous with L & RBUTTON */

        XBUTTON1     = 0x05,   /* NOT contiguous with L & RBUTTON */
        XBUTTON2     = 0x06,   /* NOT contiguous with L & RBUTTON */

        BACK         = 0x08,
        TAB          = 0x09,

        CLEAR        = 0x0C,
        RETURN       = 0x0D,

        SHIFT        = 0x10,
        CONTROL      = 0x11,
        MENU         = 0x12,
        PAUSE        = 0x13,
        CAPITAL      = 0x14,

        ESCAPE       = 0x1B,
        
        SPACE        = 0x20,
        PRIOR        = 0x21,
        NEXT         = 0x22,
        END          = 0x23,
        HOME         = 0x24,
        LEFT         = 0x25,
        UP           = 0x26,
        RIGHT        = 0x27,
        DOWN         = 0x28,
        SELECT       = 0x29,
        PRINT        = 0x2A,
        EXECUTE      = 0x2B,
        SNAPSHOT     = 0x2C,
        INSERT       = 0x2D,
        DELETE       = 0x2E,
        HELP         = 0x2F,
        
        // 0 - 9 are the same as ASCII '0' - '9' (0x30 - 0x39)
        // A - Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)

        LWIN         = 0x5B,
        RWIN         = 0x5C,
        APPS         = 0x5D,

        SLEEP        = 0x5F,

        NUMPAD0      = 0x60,
        NUMPAD1      = 0x61,
        NUMPAD2      = 0x62,
        NUMPAD3      = 0x63,
        NUMPAD4      = 0x64,
        NUMPAD5      = 0x65,
        NUMPAD6      = 0x66,
        NUMPAD7      = 0x67,
        NUMPAD8      = 0x68,
        NUMPAD9      = 0x69,
        MULTIPLY     = 0x6A,
        ADD          = 0x6B,
        SEPARATOR    = 0x6C,
        SUBTRACT     = 0x6D,
        DECIMAL      = 0x6E,
        DIVIDE       = 0x6F,
        F1           = 0x70,
        F2           = 0x71,
        F3           = 0x72,
        F4           = 0x73,
        F5           = 0x74,
        F6           = 0x75,
        F7           = 0x76,
        F8           = 0x77,
        F9           = 0x78,
        F10          = 0x79,
        F11          = 0x7A,
        F12          = 0x7B,
        F13          = 0x7C,
        F14          = 0x7D,
        F15          = 0x7E,
        F16          = 0x7F,
        F17          = 0x80,
        F18          = 0x81,
        F19          = 0x82,
        F20          = 0x83,
        F21          = 0x84,
        F22          = 0x85,
        F23          = 0x86,
        F24          = 0x87,

        NUMLOCK      = 0x90,
        SCROLL       = 0x91,

        /*
         * L* & R* - left and right Alt, Ctrl and Shift virtual keys.
         * Used only as parameters to GetAsyncKeyState() and GetKeyState().
         * No other API or message will distinguish left and right keys in this way.
         */
        LSHIFT       = 0xA0,
        RSHIFT       = 0xA1,
        LCONTROL     = 0xA2,
        RCONTROL     = 0xA3,
        LMENU        = 0xA4,
        RMENU        = 0xA5,

        OEM_1        = 0xBA,   // ';:' for US
        OEM_PLUS     = 0xBB,   // '+' any country
        OEM_COMMA    = 0xBC,   // ',' any country
        OEM_MINUS    = 0xBD,   // '-' any country
        OEM_PERIOD   = 0xBE,   // '.' any country
        OEM_2        = 0xBF,   // '/?' for US
        OEM_3        = 0xC0,   // '`~' for US

        OEM_4        = 0xDB,  //  '[{' for US
        OEM_5        = 0xDC,  //  '\|' for US
        OEM_6        = 0xDD,  //  ']}' for US
        OEM_7        = 0xDE,  //  ''"' for US
        OEM_8        = 0xDF,
        
        Last = 0xFF, // last defined key is OEM_CLEAR with value 0xFE
    }

    #endregion

    #region MouseKeys

    /// <summary>
    /// Enumerates available mouse keys (suitable for use in WM_MOUSEMOVE messages).
    /// </summary>
    enum MouseKeys
    {
        // Summary:
        //     No mouse button was pressed.
        None = 0,
        //
        // Summary:
        //     The left mouse button was pressed.
        Left = 0x0001,
        //
        // Summary:
        //     The right mouse button was pressed.
        Right = 0x0002,
        //
        // Summary:
        //     The middle mouse button was pressed.
        Middle = 0x0010,
        //
        // Summary:
        //     The first XButton was pressed.
        XButton1 = 0x0020,
        //
        // Summary:
        //     The second XButton was pressed.
        XButton2 = 0x0040,
    }

    #endregion

    #region WindowMessage

    internal enum WindowMessage : uint
    {
        NULL = 0x0000,
        CREATE = 0x0001,
        DESTROY = 0x0002,
        MOVE = 0x0003,
        SIZE = 0x0005,
        ACTIVATE = 0x0006,
        SETFOCUS = 0x0007,
        KILLFOCUS = 0x0008,
        //              internal const uint SETVISIBLE           = 0x0009;
        ENABLE = 0x000A,
        SETREDRAW = 0x000B,
        SETTEXT = 0x000C,
        GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E,
        PAINT = 0x000F,
        CLOSE = 0x0010,
        QUERYENDSESSION = 0x0011,
        QUIT = 0x0012,
        QUERYOPEN = 0x0013,
        ERASEBKGND = 0x0014,
        SYSCOLORCHANGE = 0x0015,
        ENDSESSION = 0x0016,
        //              internal const uint SYSTEMERROR          = 0x0017;
        SHOWWINDOW = 0x0018,
        CTLCOLOR = 0x0019,
        WININICHANGE = 0x001A,
        SETTINGCHANGE = 0x001A,
        DEVMODECHANGE = 0x001B,
        ACTIVATEAPP = 0x001C,
        FONTCHANGE = 0x001D,
        TIMECHANGE = 0x001E,
        CANCELMODE = 0x001F,
        SETCURSOR = 0x0020,
        MOUSEACTIVATE = 0x0021,
        CHILDACTIVATE = 0x0022,
        QUEUESYNC = 0x0023,
        GETMINMAXINFO = 0x0024,
        PAINTICON = 0x0026,
        ICONERASEBKGND = 0x0027,
        NEXTDLGCTL = 0x0028,
        //              internal const uint ALTTABACTIVE         = 0x0029;
        SPOOLERSTATUS = 0x002A,
        DRAWITEM = 0x002B,
        MEASUREITEM = 0x002C,
        DELETEITEM = 0x002D,
        VKEYTOITEM = 0x002E,
        CHARTOITEM = 0x002F,
        SETFONT = 0x0030,
        GETFONT = 0x0031,
        SETHOTKEY = 0x0032,
        GETHOTKEY = 0x0033,
        //              internal const uint FILESYSCHANGE        = 0x0034;
        //              internal const uint ISACTIVEICON         = 0x0035;
        //              internal const uint QUERYPARKICON        = 0x0036;
        QUERYDRAGICON = 0x0037,
        COMPAREITEM = 0x0039,
        //              internal const uint TESTING              = 0x003a;
        //              internal const uint OTHERWINDOWCREATED = 0x003c;
        GETOBJECT = 0x003D,
        //                      internal const uint ACTIVATESHELLWINDOW        = 0x003e;
        COMPACTING = 0x0041,
        COMMNOTIFY = 0x0044,
        WINDOWPOSCHANGING = 0x0046,
        WINDOWPOSCHANGED = 0x0047,
        POWER = 0x0048,
        COPYDATA = 0x004A,
        CANCELJOURNAL = 0x004B,
        NOTIFY = 0x004E,
        INPUTLANGCHANGEREQUEST = 0x0050,
        INPUTLANGCHANGE = 0x0051,
        TCARD = 0x0052,
        HELP = 0x0053,
        USERCHANGED = 0x0054,
        NOTIFYFORMAT = 0x0055,
        CONTEXTMENU = 0x007B,
        STYLECHANGING = 0x007C,
        STYLECHANGED = 0x007D,
        DISPLAYCHANGE = 0x007E,
        GETICON = 0x007F,

        // Non-Client messages
        SETICON = 0x0080,
        NCCREATE = 0x0081,
        NCDESTROY = 0x0082,
        NCCALCSIZE = 0x0083,
        NCHITTEST = 0x0084,
        NCPAINT = 0x0085,
        NCACTIVATE = 0x0086,
        GETDLGCODE = 0x0087,
        SYNCPAINT = 0x0088,
        //              internal const uint SYNCTASK       = 0x0089;
        NCMOUSEMOVE = 0x00A0,
        NCLBUTTONDOWN = 0x00A1,
        NCLBUTTONUP = 0x00A2,
        NCLBUTTONDBLCLK = 0x00A3,
        NCRBUTTONDOWN = 0x00A4,
        NCRBUTTONUP = 0x00A5,
        NCRBUTTONDBLCLK = 0x00A6,
        NCMBUTTONDOWN = 0x00A7,
        NCMBUTTONUP = 0x00A8,
        NCMBUTTONDBLCLK = 0x00A9,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONDOWN    = 0x00ab,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONUP      = 0x00ac,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        NCXBUTTONDBLCLK  = 0x00ad,

        INPUT = 0x00FF,
        
        KEYDOWN = 0x0100,
        KEYFIRST = 0x0100,
        KEYUP = 0x0101,
        CHAR = 0x0102,
        DEADCHAR = 0x0103,
        SYSKEYDOWN = 0x0104,
        SYSKEYUP = 0x0105,
        SYSCHAR = 0x0106,
        SYSDEADCHAR = 0x0107,
        KEYLAST = 0x0108,
        IME_STARTCOMPOSITION = 0x010D,
        IME_ENDCOMPOSITION = 0x010E,
        IME_COMPOSITION = 0x010F,
        IME_KEYLAST = 0x010F,
        INITDIALOG = 0x0110,
        COMMAND = 0x0111,
        SYSCOMMAND = 0x0112,
        TIMER = 0x0113,
        HSCROLL = 0x0114,
        VSCROLL = 0x0115,
        INITMENU = 0x0116,
        INITMENUPOPUP = 0x0117,
        //              internal const uint SYSTIMER       = 0x0118;
        MENUSELECT = 0x011F,
        MENUCHAR = 0x0120,
        ENTERIDLE = 0x0121,
        MENURBUTTONUP = 0x0122,
        MENUDRAG = 0x0123,
        MENUGETOBJECT = 0x0124,
        UNINITMENUPOPUP = 0x0125,
        MENUCOMMAND = 0x0126,

        CHANGEUISTATE = 0x0127,
        UPDATEUISTATE = 0x0128,
        QUERYUISTATE = 0x0129,

        //              internal const uint LBTRACKPOINT     = 0x0131;
        CTLCOLORMSGBOX = 0x0132,
        CTLCOLOREDIT = 0x0133,
        CTLCOLORLISTBOX = 0x0134,
        CTLCOLORBTN = 0x0135,
        CTLCOLORDLG = 0x0136,
        CTLCOLORSCROLLBAR = 0x0137,
        CTLCOLORSTATIC = 0x0138,
        MOUSEMOVE = 0x0200,
        MOUSEFIRST = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        LBUTTONDBLCLK = 0x0203,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MBUTTONDBLCLK = 0x0209,
        MOUSEWHEEL = 0x020A,
        MOUSELAST = 0x020D,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONDOWN        = 0x020B,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONUP        = 0x020C,
        /// <summary>
        /// Windows 2000 and higher only.
        /// </summary>
        XBUTTONDBLCLK    = 0x020D,
        PARENTNOTIFY = 0x0210,
        ENTERMENULOOP = 0x0211,
        EXITMENULOOP = 0x0212,
        NEXTMENU = 0x0213,
        SIZING = 0x0214,
        CAPTURECHANGED = 0x0215,
        MOVING = 0x0216,
        //              internal const uint POWERBROADCAST   = 0x0218;
        DEVICECHANGE = 0x0219,
        MDICREATE = 0x0220,
        MDIDESTROY = 0x0221,
        MDIACTIVATE = 0x0222,
        MDIRESTORE = 0x0223,
        MDINEXT = 0x0224,
        MDIMAXIMIZE = 0x0225,
        MDITILE = 0x0226,
        MDICASCADE = 0x0227,
        MDIICONARRANGE = 0x0228,
        MDIGETACTIVE = 0x0229,
        /* D&D messages */
        //              internal const uint DROPOBJECT     = 0x022A;
        //              internal const uint QUERYDROPOBJECT  = 0x022B;
        //              internal const uint BEGINDRAG      = 0x022C;
        //              internal const uint DRAGLOOP       = 0x022D;
        //              internal const uint DRAGSELECT     = 0x022E;
        //              internal const uint DRAGMOVE       = 0x022F;
        MDISETMENU = 0x0230,
        ENTERSIZEMOVE = 0x0231,
        EXITSIZEMOVE = 0x0232,
        DROPFILES = 0x0233,
        MDIREFRESHMENU = 0x0234,
        IME_SETCONTEXT = 0x0281,
        IME_NOTIFY = 0x0282,
        IME_CONTROL = 0x0283,
        IME_COMPOSITIONFULL = 0x0284,
        IME_SELECT = 0x0285,
        IME_CHAR = 0x0286,
        IME_REQUEST = 0x0288,
        IME_KEYDOWN = 0x0290,
        IME_KEYUP = 0x0291,
        NCMOUSEHOVER = 0x02A0,
        MOUSEHOVER = 0x02A1,
        NCMOUSELEAVE = 0x02A2,
        MOUSELEAVE = 0x02A3,
        CUT = 0x0300,
        COPY = 0x0301,
        PASTE = 0x0302,
        CLEAR = 0x0303,
        UNDO = 0x0304,
        RENDERFORMAT = 0x0305,
        RENDERALLFORMATS = 0x0306,
        DESTROYCLIPBOARD = 0x0307,
        DRAWCLIPBOARD = 0x0308,
        PAINTCLIPBOARD = 0x0309,
        VSCROLLCLIPBOARD = 0x030A,
        SIZECLIPBOARD = 0x030B,
        ASKCBFORMATNAME = 0x030C,
        CHANGECBCHAIN = 0x030D,
        HSCROLLCLIPBOARD = 0x030E,
        QUERYNEWPALETTE = 0x030F,
        PALETTEISCHANGING = 0x0310,
        PALETTECHANGED = 0x0311,
        HOTKEY = 0x0312,
        PRINT = 0x0317,
        PRINTCLIENT = 0x0318,
        HANDHELDFIRST = 0x0358,
        HANDHELDLAST = 0x035F,
        AFXFIRST = 0x0360,
        AFXLAST = 0x037F,
        PENWINFIRST = 0x0380,
        PENWINLAST = 0x038F,
        APP = 0x8000,
        USER = 0x0400,

        // Our "private" ones
        MOUSE_ENTER = 0x0401,
        ASYNC_MESSAGE = 0x0403,
        REFLECT = USER + 0x1c00,
        CLOSE_INTERNAL = USER + 0x1c01,

        // NotifyIcon (Systray) Balloon messages 
        BALLOONSHOW = USER + 0x0002,
        BALLOONHIDE = USER + 0x0003,
        BALLOONTIMEOUT = USER + 0x0004,
        BALLOONUSERCLICK = USER + 0x0005
    }        

    #endregion

    #region ShowWindowCommand

    /// <summary>
    /// ShowWindow() Commands
    /// </summary>
    internal enum ShowWindowCommand
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        HIDE            = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
        /// </summary>
        SHOWNORMAL      = 1,
        NORMAL          = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        SHOWMINIMIZED   = 2,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        SHOWMAXIMIZED   = 3,
        MAXIMIZE        = 3,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        SHOWNOACTIVATE  = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        SHOW            = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        MINIMIZE        = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        SHOWMINNOACTIVE = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is similar to SW_SHOW, except the window is not activated.
        /// </summary>
        SHOWNA          = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
        /// </summary>
        RESTORE         = 9,
        /// <summary>
        /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.
        /// </summary>
        SHOWDEFAULT     = 10,
        /// <summary>
        /// Windows 2000/XP: Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
        /// </summary>
        FORCEMINIMIZE   = 11,
        //MAX             = 11,

        // Old ShowWindow() Commands
        //HIDE_WINDOW        = 0,
        //SHOW_OPENWINDOW    = 1,
        //SHOW_ICONWINDOW    = 2,
        //SHOW_FULLSCREEN    = 3,
        //SHOW_OPENNOACTIVATE= 4,
    }

    #endregion

    #region MapVirtualKeyType

    internal enum MapVirtualKeyType
    {
        /// <summary>uCode is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.</summary>
        VirtualKeyToScanCode = 0,
        /// <summary>uCode is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.</summary>
        ScanCodeToVirtualKey = 1,
        /// <summary>uCode is a virtual-key code and is translated into an unshifted character value in the low-order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.</summary>
        VirtualKeyToCharacter = 2,
        /// <summary>Windows NT/2000/XP: uCode is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.</summary>
        ScanCodeToVirtualKeyExtended = 3,
        VirtualKeyToScanCodeExtended = 4,
    }

    #endregion

    #region MonitorFrom

    enum MonitorFrom
    {
        Null = 0,
        Primary = 1,
        Nearest = 2,
    }

    #endregion

    #region CursorName

    enum CursorName : int
    {
        Arrow = 32512
    }

    #endregion

    #region TrackMouseEventFlags

    [Flags]
    enum TrackMouseEventFlags : uint
    {
        HOVER = 0x00000001,
        LEAVE = 0x00000002,
        NONCLIENT = 0x00000010,
        QUERY = 0x40000000,
        CANCEL = 0x80000000,
    }

    #endregion

    #endregion

    #region --- Callbacks ---

    internal delegate IntPtr WindowProcedure(IntPtr handle, WindowMessage message, IntPtr wParam, IntPtr lParam);

    #region Message

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG
    {
        internal IntPtr HWnd;
        internal WindowMessage Message;
        internal IntPtr WParam;
        internal IntPtr LParam;
        internal uint Time;
        internal POINT Point;
        //internal object RefObject;

        public override string ToString()
        {
            return String.Format("msg=0x{0:x} ({1}) hwnd=0x{2:x} wparam=0x{3:x} lparam=0x{4:x} pt=0x{5:x}", (int)Message, Message.ToString(), HWnd.ToInt32(), WParam.ToInt32(), LParam.ToInt32(), Point);
        }
    }

    #endregion

    #region Point

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        internal int X;
        internal int Y;

        internal POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        internal Point ToPoint()
        {
            return new Point(X, Y);
        }

        public override string ToString()
        {
            return "Point {" + X.ToString() + ", " + Y.ToString() + ")";
        }
    }

    #endregion

    #endregion
}

#pragma warning restore 3019
#pragma warning restore 0649
#pragma warning restore 0169
#pragma warning restore 0414