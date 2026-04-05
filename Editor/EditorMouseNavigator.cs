using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    /// <summary>
    /// Polls mouse side buttons (Back/Forward) editor-wide using native APIs,
    /// and navigates selection history with context-aware filtering based on Ctrl key state.
    /// 
    /// - Mouse Back/Forward (without Ctrl): Navigate Asset entries only (Project View context)
    /// - Ctrl + Mouse Back/Forward: Navigate Scene Object entries only (Hierarchy View context)
    /// 
    /// Reference: GitHub Issue #27, polling approach by lgarczyn
    /// </summary>
    [InitializeOnLoad]
    public static class EditorMouseNavigator
    {
#if UNITY_EDITOR_WIN
        // Windows P/Invoke — async polling for mouse/keyboard state
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Virtual key codes
        private const int VK_XBUTTON1 = 0x05;  // Mouse Back button
        private const int VK_XBUTTON2 = 0x06;  // Mouse Forward button
        private const int VK_CONTROL  = 0x11;  // Ctrl key

#elif UNITY_EDITOR_OSX
        // macOS: Poll mouse button state via NSEvent.pressedMouseButtons
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern IntPtr sel_registerName(string name);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern uint objc_msgSend_uint(IntPtr receiver, IntPtr selector);

        // macOS: Poll Ctrl key state via NSEvent.modifierFlags
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        private static extern ulong objc_msgSend_ulong(IntPtr receiver, IntPtr selector);

        // NSEventModifierFlagControl = 1 << 18
        private const ulong NSEventModifierFlagControl = 1UL << 18;
#endif

        // Previous frame button state — used for rising edge detection.
        // Initialized to true to prevent false triggers if buttons are already held at editor startup.
        private static bool s_BackPressed = true;
        private static bool s_ForwardPressed = true;

        static EditorMouseNavigator()
        {
            EditorApplication.update += PollMouseButtons;
        }

        private static void PollMouseButtons()
        {
            if (SelectionHistoryPreferences.nativeKeyHandleDisabled)
            {
                return;
            }
            
            bool backState = false;
            bool forwardState = false;
            bool ctrlState = false;

            try
            {
#if UNITY_EDITOR_WIN
                // Windows: GetAsyncKeyState — bit 0x8000 is set if key is currently pressed
                backState    = (GetAsyncKeyState(VK_XBUTTON1) & 0x8000) != 0;
                forwardState = (GetAsyncKeyState(VK_XBUTTON2) & 0x8000) != 0;
                ctrlState    = (GetAsyncKeyState(VK_CONTROL)  & 0x8000) != 0;
                
#elif UNITY_EDITOR_OSX
                // macOS: NSEvent.pressedMouseButtons bitmask
                IntPtr nsEventClass = objc_getClass("NSEvent");
                IntPtr pressedSel = sel_registerName("pressedMouseButtons");
                uint pressedButtons = objc_msgSend_uint(nsEventClass, pressedSel);

                // Bit 3 = Mouse Button 4 (Back), Bit 4 = Mouse Button 5 (Forward)
                backState    = (pressedButtons & (1u << 3)) != 0;
                forwardState = (pressedButtons & (1u << 4)) != 0;

                // macOS: Check Ctrl state via NSEvent.modifierFlags
                IntPtr modSel = sel_registerName("modifierFlags");
                ulong modFlags = objc_msgSend_ulong(nsEventClass, modSel);
                ctrlState = (modFlags & NSEventModifierFlagControl) != 0;
#endif
            }
            catch
            {
                // Silently ignore native call failures — will retry next frame
                return;
            }

            // Rising edge detection: was released last frame, now pressed = click event
            if (backState && !s_BackPressed)
            {
                HandleNavigation(isForward: false, isCtrlHeld: ctrlState);
            }

            if (forwardState && !s_ForwardPressed)
            {
                HandleNavigation(isForward: true, isCtrlHeld: ctrlState);
            }

            // Save state for next frame comparison
            s_BackPressed = backState;
            s_ForwardPressed = forwardState;
        }

        /// <summary>
        /// Handles mouse button click — determines filtering context based on Ctrl key state.
        /// Wrapped in delayCall to safely modify Selection from main editor loop.
        /// </summary>
        private static void HandleNavigation(bool isForward, bool isCtrlHeld)
        {
            EditorApplication.delayCall += () =>
            {
                var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                if (selectionHistory == null || selectionHistory.History.Count == 0)
                    return;

                // Without Ctrl: Asset (Project View) context
                // With Ctrl:    Scene Object (Hierarchy View) context
                Func<SelectionHistory.Entry, bool> predicate;
                if (isCtrlHeld)
                    predicate = entry => entry.isSceneInstance; // Hierarchy: Scene objects only
                else
                    predicate = entry => entry.isAsset;        // Project: Asset objects only

                bool found = isForward
                    ? selectionHistory.NextFiltered(predicate)
                    : selectionHistory.PreviousFiltered(predicate);

                if (found)
                {
                    Selection.activeObject = selectionHistory.GetSelection();
                }
            };
        }
    }
}
