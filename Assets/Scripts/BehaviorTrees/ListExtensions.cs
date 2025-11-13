using System;
using System.Collections.Generic;
using System.Linq; // Needed for .ToList() if you convert to List

public static class ListExtensions // Define in a static class
{
    private static Random rng = new Random(); // Use System.Random for basic randomness

    public static List<T> Shuffle<T>(this List<T> list)
    {
        // Fisher-Yates shuffle
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // Get a random index from 0 to n
            T value = list[k];       // Swap the element at k with the element at n
            list[k] = list[n];
            list[n] = value;
        }
        return list; // Return the shuffled list (it shuffles in-place)
    }

    // A version that creates a new shuffled list (non-destructive)
    public static List<T> ShuffleNew<T>(this List<T> list)
    {
        List<T> newList = new List<T>(list); // Create a copy
        Shuffle(newList); // Shuffle the copy
        return newList;
    }
}