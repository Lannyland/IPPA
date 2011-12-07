using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    public class Permutation
    {
        #region Members

        // Private variables
        private int[] data = null;
        private int n = 0;
        private List<int[]> result = new List<int[]>();

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public Permutation(int _n)
        {
            n = _n;

            // Sanity check. Don't do it if n too large
            if (n > 10)
            {
                System.Windows.Forms.MessageBox.Show("n=" + n + " n is larger than 10, I won't do it!");
                return;
            }

            Permutate();
        }

        // Destructor
        ~Permutation()
        {
            // Cleaning up
        }
        #endregion

        #region Other Functions

        // Actually performing the permutation
        private void Permutate()
        {
            // First create identy
            data = new int[n];
            for (int i = 0; i < n; i++)
            {
                data[i] = i;
            }
            result.Add(data);

            // Now let's do pair swap
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    int[] newP = new int[n];
                    // Duplicate data
                    for (int k = 0; k < n; k++)
                    {
                        newP[k] = data[k];
                    }
                    // Swap
                    newP[i] = data[j];
                    newP[j] = data[i];
                    result.Add(newP);
                }
            }

            int[] newP2 = new int[n];
            for (int k = 0; k < n; k++)
            {
                newP2[k] = data[n-1-k];
            }

            CheckForDup(newP2);

            // Now let's do pair swap
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    int[] newP3 = new int[n];
                    // Duplicate data
                    for (int k = 0; k < n; k++)
                    {
                        newP3[k] = newP2[k];
                    }
                    // Swap
                    newP3[i] = newP2[j];
                    newP3[j] = newP2[i];
                    CheckForDup(newP3);
                }
            }


        }

        private void CheckForDup(int[] newP)
        {
            bool dupe = false;
            int l = result.Count();
            for (int i = 0; i < l; i++ )
            {
                if (intarraytostr(result[i]) == intarraytostr(newP))
                {
                    dupe = true;
                }
            }
            if (!dupe)
            {
                result.Add(newP);
            }
        }

        private string intarraytostr(int[] a)
        {
            string s = "";
            for (int i = 0; i < a.Length; i++)
            {
                s += a[i].ToString();
            }
            return s;
        }

        // Return permutation result
        public List<int[]> GetResult()
        {
            return result;
        }

        // Debug code
        public void PrintResult()
        {
            for (int i = 0; i < result.Count(); i++)
            {
                for (int j = 0; j < result[i].Length; j++)
                {
                    Console.Write(result[i][j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        #endregion
    }


    public class PermuteUtils
    {
        // Returns an enumeration of enumerators, one for each permutation
        // of the input.
        public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list, int count)
        {
            if (count == 0)
            {
                yield return new T[0];
            }
            else
            {
                int startingElementIndex = 0;
                foreach (T startingElement in list)
                {
                    IEnumerable<T> remainingItems = AllExcept(list, startingElementIndex);

                    foreach (IEnumerable<T> permutationOfRemainder in Permute(remainingItems, count - 1))
                    {
                        yield return Concat<T>(
                            new T[] { startingElement },
                            permutationOfRemainder);
                    }
                    startingElementIndex += 1;
                }
            }
        }

        // Enumerates over contents of both lists.
        public static IEnumerable<T> Concat<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            foreach (T item in a) { yield return item; }
            foreach (T item in b) { yield return item; }
        }

        // Enumerates over all items in the input, skipping over the item
        // with the specified offset.
        public static IEnumerable<T> AllExcept<T>(IEnumerable<T> input, int indexToSkip)
        {
            int index = 0;
            foreach (T item in input)
            {
                if (index != indexToSkip) yield return item;
                index += 1;
            }
        }
    }
}
