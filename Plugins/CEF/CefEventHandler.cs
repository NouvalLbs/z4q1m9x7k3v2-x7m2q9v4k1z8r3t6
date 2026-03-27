using ProjectSMP;
using ProjectSMP.Plugins.CEF;
using SampSharp.GameMode.World;
using System;

public static class CefEventHandler
{
    public static void Initialize() {
        CefService.OnClientEvent += Handle;
    }

    private static void Handle(int playerId, string eventName, string payload) {
        var player = BasePlayer.Find(playerId) as Player;
        if (player == null) return;

        switch (eventName) {
            case "uiReady": HandleUiReady(player); break;
        }
    }

    private static void HandleUiReady(Player player) {
        //TODO
    }
}