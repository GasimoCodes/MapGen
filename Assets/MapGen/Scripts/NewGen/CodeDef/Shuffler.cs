using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public static class Shuffler
    {

        //Shuffle array (Is local cuz we realllyyyy need the seed to stay deterministic
        public static T[] Shuffle<T>(T[] array, System.Random randomGen)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                int rnd = randomGen.Next(i, array.Length);
                T tempGO = array[rnd];
                array[rnd] = array[i];
                array[i] = tempGO;
            }
            return array;
        }

        public static List<T> Shuffle<T>(List<T> list, System.Random randomGen)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int rnd = randomGen.Next(i, list.Count);
                T tempGO = list[rnd];
                list[rnd] = list[i];
                list[i] = tempGO;
            }
            return list;
        }

    }
}