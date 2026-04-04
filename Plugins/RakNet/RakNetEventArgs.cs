using System;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetEventArgs : EventArgs
    {
        public int PlayerId { get; }
        public int Id { get; }
        public BitStream BitStream { get; }
        public bool PreventDefault { get; set; }

        public RakNetEventArgs(int playerId, int id, BitStream bs)
        {
            PlayerId = playerId;
            Id = id;
            BitStream = bs;
            PreventDefault = false;
        }
    }
}