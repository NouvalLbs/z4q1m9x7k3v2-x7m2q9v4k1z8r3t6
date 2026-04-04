using System;

namespace ProjectSMP.Plugins.RakNet
{
    public static class RakNetUtilities
    {
        public static int BitsToBytes(int bits) => ((bits + 7) >> 3);
        public static int BytesToBits(int bytes) => (bytes << 3);

        public static int PackAspectRatio(float value)
        {
            return (int)Math.Round((value - 1.0f) * 255.0f);
        }

        public static float UnpackAspectRatio(int value)
        {
            return (float)value / 255.0f + 1.0f;
        }

        public static int PackCameraZoom(float value)
        {
            return (int)Math.Round(((value - 35.0f) / 35.0f) * 63.0f);
        }

        public static float UnpackCameraZoom(int value)
        {
            return ((float)value / 63.0f) * 35.0f + 35.0f;
        }

        public static byte PackHealthArmour(int health, int armour)
        {
            byte healthArmour = 0;

            if (health > 0 && health < 100)
            {
                healthArmour = (byte)((health / 7) << 4);
            }
            else if (health >= 100)
            {
                healthArmour = 0xF0;
            }

            if (armour > 0 && armour < 100)
            {
                healthArmour |= (byte)(armour / 7);
            }
            else if (armour >= 100)
            {
                healthArmour |= 0xF;
            }

            return healthArmour;
        }

        public static void UnpackHealthArmour(byte healthArmour, out int health, out int armour)
        {
            health = healthArmour >> 4;
            if (health == 0xF)
            {
                health = 100;
            }
            else
            {
                health *= 7;
            }

            armour = healthArmour & 0xF;
            if (armour == 0xF)
            {
                armour = 100;
            }
            else
            {
                armour *= 7;
            }
        }
    }
}