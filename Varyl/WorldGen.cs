using System;

namespace Varyl
{
    public class WorldGen
    {
        string worldname;
        private Requirements requirement = null;
        private int Width = 0;
        private int Height = 0;
        private int Countries = 1;
        private int Religions = 0;

        private class Requirements
        {
            int minLevel;
            int maxLevel;
        }
    }

    public class CharacterGen
    {
        string name;
        int level;
        int type = 0; //NPC, Merchant, Adventurer
        int age = 18;
        Stats stats;
        Class @class;
        Design design;

        private class Design
        {
            int HairLength = 0;
            int HairColor = 0;
            int Size = 180;

        }
        private enum Class
        {
            Mage,
            Warrior,
            Rogue
        }

        private class Stats
        {
            int Strength = 5;
            int Wisdom = 5;
            int Intelligence = 5;
            int Agility = 5;
            int Luck = 5;
            int Seed = 898243983;
            Stats(Class _class, CharacterGen gen)
            {
                Random random = new Random(Seed);
                random = new Random(random.Next());

                switch (_class)
                {
                    case Class.Mage:
                        Strength = gen.level * 2;
                        Wisdom = gen.level * 4;
                        Intelligence = gen.level * 7;
                        Agility = gen.level * 3;
                        Luck = (int)(gen.level * 2.4);
                        break;
                    case Class.Rogue:
                        break;
                    case Class.Warrior:
                        break;
                }
            }
        }
    }

    public static class RandomExtension
    {
        public static float NextFloat(this Random random, float min, float max)
        {
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }
    }

}
