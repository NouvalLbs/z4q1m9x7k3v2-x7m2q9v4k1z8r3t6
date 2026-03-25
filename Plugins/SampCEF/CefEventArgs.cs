using System;

namespace ProjectSMP.Plugins.SampCEF
{
    public class CefInitializeEventArgs : EventArgs
    {
        public int PlayerId { get; }
        public bool Success { get; }

        public CefInitializeEventArgs(int playerId, bool success)
        {
            PlayerId = playerId;
            Success = success;
        }
    }

    public class CefBrowserCreatedEventArgs : EventArgs
    {
        public int PlayerId { get; }
        public int BrowserId { get; }
        public int StatusCode { get; }

        public CefBrowserCreatedEventArgs(int playerId, int browserId, int statusCode)
        {
            PlayerId = playerId;
            BrowserId = browserId;
            StatusCode = statusCode;
        }
    }
}