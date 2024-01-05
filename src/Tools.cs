using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Npgsql;

namespace KeyMan
{
    public static class GeneralTools
    {
        public static void Swap<T>(ref T val, ref T val1)
        {
            T temp = val;

            val = val1;
            val1 = val;
        }

        /// <summary>
        /// Clips the provided int to the nearest maximum or minimum value.
        /// </summary>
        /// <param name="val"> Value to be clipped. </param>
        /// <param name="min"> Minimum value. </param>
        /// <param name="max"> Maximum value. </param>
        /// <returns></returns>
        public static long Clip(long val, long min, long max)
        {
            if (val < min)
                return min;
            else if (val > max)
                return max;

            return val;
        }

        public static void ArrayCopy<T>(ref T[] source, ref T[] destination)
        {
            int size = source.Length;

            for (int x = 0; x < size; x++)
                destination[x] = source[x];

            GeneralTools.PrintArray<T>(destination);
        }

        public static bool InRange(long val, long min, long max)
        {
            return (val >= min && val < max);
        }

        public static int Clip(ref int num, int min, int max)
        {
            if (num  < min)
                num = min;

            else if (num > max)
                num = max;

            return num;
        }

        public static void PrintArray<T>(T[] array)
        {
            int size = array.Length;

            Console.Write("{ ");

            for (int x = 0; x < size; x++)
                Console.Write((x == size - 1) ? $"{array[x]}" : $"{array[x]}, ");

            Console.Write(" }");
        }

        public static int DateTimeCmp(DateTime dateTime, DateTime dateTime1)
        {
            int[,] dateTimeMatrix = new int[2, 6]{
                { dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second },
                { dateTime1.Year, dateTime1.Month, dateTime1.Day, dateTime1.Hour, dateTime1.Minute, dateTime1.Second },
            };

            for (int x = 0; x < 6; x++)
                if (dateTimeMatrix[0, x] > dateTimeMatrix[1, x])
                    return 1;
                else if (dateTimeMatrix[0, x] < dateTimeMatrix[1, x])
                    return -1;

            return 0;
        }

        public static string ConvertBytesToString(Byte[] bytes)
        {
            string str = null;

            int size = bytes.Length;

            for (int x = 0; x < size; x++)
                str += bytes[x].ToString("x2");

            return str;
        }

        public static string GetRandomBase64(int size)
        {
            byte[] bytes = new byte[size];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }

            return null; 
        }
    }

    public static class KeyTools
    {
        public static string GetPermissionsString(Dictionary<string, bool> permissionsMap)
        {
            string permissionsString = null;

            string[] keys = permissionsMap.Keys.ToArray();

            bool[] values = permissionsMap.Values.ToArray();

            for (int x = 0; x < keys.Length; x++)
                permissionsString += $"{keys[x]}={values[x]};";

            return permissionsString;
        }

        public static Dictionary<string, bool> GetPermissionsMap(string permissionsString)
        {
            Dictionary<string, bool> permissionsMap = new Dictionary<string, bool>();

            string[] split = permissionsString.Split(';'), split1;

            for (int x = 0; x < split.Length - 1; x++)
                permissionsMap.Add((split1 = split[x].Split('='))[0], split1[1] == "True");

            return permissionsMap;
        } 
    }
}

 