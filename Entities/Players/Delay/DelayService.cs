#nullable enable
using System;

namespace ProjectSMP.Entities.Players.Delay
{
    public static class DelayService
    {
        public static void SetQuitJobDelay(Player player, int days)
        {
            var expireTime = DateTime.UtcNow.AddDays(days);
            player.Delays.QuitJob = expireTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool HasQuitJobDelay(Player player)
        {
            if (string.IsNullOrEmpty(player.Delays.QuitJob))
                return false;

            if (!DateTime.TryParse(player.Delays.QuitJob, out var expireTime))
                return false;

            return DateTime.UtcNow < expireTime;
        }

        public static TimeSpan GetQuitJobRemainingTime(Player player)
        {
            if (string.IsNullOrEmpty(player.Delays.QuitJob) ||
                !DateTime.TryParse(player.Delays.QuitJob, out var expireTime))
                return TimeSpan.Zero;

            var remaining = expireTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public static void SetJobDelay(Player player, string jobType, int delayMinutes)
        {
            switch (jobType.ToLower())
            {
                case "lumber": player.Delays.Lumber = delayMinutes; break;
                case "product": player.Delays.Product = delayMinutes; break;
                case "truckerhauling": player.Delays.TruckerHauling = delayMinutes; break;
                case "truckermission": player.Delays.TruckerMission = delayMinutes; break;
                case "truckercontainer": player.Delays.TruckerContainer = delayMinutes; break;
                case "truckercrate": player.Delays.TruckerCrate = delayMinutes; break;
                case "forager": player.Delays.Forager = delayMinutes; break;
                case "farm": player.Delays.Farm = delayMinutes; break;
                case "sweeper": player.Delays.Sweeper = delayMinutes; break;
                case "courir": player.Delays.Courir = delayMinutes; break;
                case "forklifter": player.Delays.Forklifter = delayMinutes; break;
                case "bus": player.Delays.Bus = delayMinutes; break;
                case "trashmaster": player.Delays.Trashmaster = delayMinutes; break;
                case "mower": player.Delays.Mower = delayMinutes; break;
                case "fisherman": player.Delays.Fisherman = delayMinutes; break;
                case "honey": player.Delays.Honey = delayMinutes; break;
                case "miner": player.Delays.Miner = delayMinutes; break;
            }
        }

        public static int GetJobDelay(Player player, string jobType)
        {
            return jobType.ToLower() switch
            {
                "lumber" => player.Delays.Lumber,
                "product" => player.Delays.Product,
                "truckerhauling" => player.Delays.TruckerHauling,
                "truckermission" => player.Delays.TruckerMission,
                "truckercontainer" => player.Delays.TruckerContainer,
                "truckercrate" => player.Delays.TruckerCrate,
                "forager" => player.Delays.Forager,
                "farm" => player.Delays.Farm,
                "sweeper" => player.Delays.Sweeper,
                "courir" => player.Delays.Courir,
                "forklifter" => player.Delays.Forklifter,
                "bus" => player.Delays.Bus,
                "trashmaster" => player.Delays.Trashmaster,
                "mower" => player.Delays.Mower,
                "fisherman" => player.Delays.Fisherman,
                "honey" => player.Delays.Honey,
                "miner" => player.Delays.Miner,
                _ => 0
            };
        }

        public static bool HasJobDelay(Player player, string jobType)
        {
            return GetJobDelay(player, jobType) > 0;
        }

        public static void ReduceAllDelays(Player player, int minutesElapsed)
        {
            player.Delays.Lumber = Math.Max(0, player.Delays.Lumber - minutesElapsed);
            player.Delays.Product = Math.Max(0, player.Delays.Product - minutesElapsed);
            player.Delays.TruckerHauling = Math.Max(0, player.Delays.TruckerHauling - minutesElapsed);
            player.Delays.TruckerMission = Math.Max(0, player.Delays.TruckerMission - minutesElapsed);
            player.Delays.TruckerContainer = Math.Max(0, player.Delays.TruckerContainer - minutesElapsed);
            player.Delays.TruckerCrate = Math.Max(0, player.Delays.TruckerCrate - minutesElapsed);
            player.Delays.Forager = Math.Max(0, player.Delays.Forager - minutesElapsed);
            player.Delays.Farm = Math.Max(0, player.Delays.Farm - minutesElapsed);
            player.Delays.Sweeper = Math.Max(0, player.Delays.Sweeper - minutesElapsed);
            player.Delays.Courir = Math.Max(0, player.Delays.Courir - minutesElapsed);
            player.Delays.Forklifter = Math.Max(0, player.Delays.Forklifter - minutesElapsed);
            player.Delays.Bus = Math.Max(0, player.Delays.Bus - minutesElapsed);
            player.Delays.Trashmaster = Math.Max(0, player.Delays.Trashmaster - minutesElapsed);
            player.Delays.Mower = Math.Max(0, player.Delays.Mower - minutesElapsed);
            player.Delays.Fisherman = Math.Max(0, player.Delays.Fisherman - minutesElapsed);
            player.Delays.Honey = Math.Max(0, player.Delays.Honey - minutesElapsed);
            player.Delays.Miner = Math.Max(0, player.Delays.Miner - minutesElapsed);
        }
    }
}