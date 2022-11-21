using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace algh
{
    class Item
    {
        public int Id { get; set; }
        public int Weight { get; set; }
        public Item(int _Id, int _weight)
        {
            Id = _Id;
            Weight = _weight;
        }
    }

    class Conteiner
    {
        public int Capacity { get; set; }
        public List<Item> ItemsTaken { get; set; }
        public int ConteinerIndex { get; set; }

        public Conteiner(int _Capacity, List<Item> _ItemsTaken,int _ConteinerIndex)
        {
            ConteinerIndex = _ConteinerIndex;
            Capacity = _Capacity;
            ItemsTaken = _ItemsTaken;
        }
    }
    class Program
    {
        public static IEnumerable<IEnumerable<int>> GetPermutations(int n)
        {
            if (n == 0)
                yield break;

            if (n == 1)
            {
                yield return new[] { 0 };
                yield break;
            }

            var currentElement = n - 1;
            foreach (var permutationWithoutCurrentElement in GetPermutations(n - 1))
            {
                for (var insertIndex = 0; insertIndex < n; insertIndex++)
                {
                    
                    var res =permutationWithoutCurrentElement
                        .Take(insertIndex)
                        .Append(currentElement)
                        .Concat(permutationWithoutCurrentElement.Skip(insertIndex));
                    yield return res;
                }
            }
        }
        public static List<Conteiner> FF(int capacity,List<Item>items)
        {
            var result = new List<Conteiner>();

            foreach (var item in items)
            {
                var itemWeight = item.Weight;
                var conteinerIndex = 0;
                while (conteinerIndex < result.Count && itemWeight + result[conteinerIndex].Capacity > capacity) 
                    conteinerIndex++;

                if(conteinerIndex == result.Count)
                {
                    result.Add(new Conteiner(0, new List<Item>(),conteinerIndex));
                }
                result[conteinerIndex].ItemsTaken.Add(item);
                result[conteinerIndex].Capacity+=itemWeight;

            }
            return result;
        }

        public static List<Conteiner> FFS(int capacity, List<Item> items)
        {
            var ItemsDesc = items.OrderByDescending(item=>item.Weight).ToList();
            var ffsres = FF(capacity, ItemsDesc);
            return ffsres;
        }
        public static Random random = new Random();

        public static void OutputInformationContainers(List<Conteiner>conteiners)
        {
            foreach (var cont in conteiners)
            {
                Console.WriteLine("Номер контейнера: " + cont.ConteinerIndex + " Вес контейнера: " + cont.Capacity);
                Console.WriteLine("--------------------------------------");
                foreach (var item in cont.ItemsTaken)
                {
                    Console.Write("Номер предмета: " + item.Id + " " + "Вес предмета: " + item.Weight + "\n");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Число использованных контейнеров: " + conteiners.Count);
            Console.WriteLine("\n");
        }

        public static void PerformanceTest(int capacity,Func<int,List<Item>,List<Conteiner>>act)
        {
            Stopwatch sw = new Stopwatch();
            int CountCorrect = 0;
            int iterN = 8;
            int countK = 100;
            int sum1 = 0;
            int sum2 = 0;
            for(int i=1;i<=iterN;i++)
            {
                for (int k = 0; k < countK; k++)
                {
                    var weights = new List<Item>();
                    for (int j = 0; j < i; j++)
                        weights.Add(new Item(j, random.Next(1, 2 * (capacity / 3))));
                    sw.Start();
                    var actual = act(capacity, weights);
                    sw.Stop();

                    var min = int.MaxValue;
                    List<Conteiner> expected = null;
                    foreach (var indexes in GetPermutations(i))
                    {
                        var currentPermutation = indexes.Select(index => weights[index]).ToList();

                        var permff = FF(capacity, currentPermutation);
                        if (permff.Count < min)
                        {
                            expected = permff;
                            min = permff.Count;
                        }
                    }
                    if (expected.Count == actual.Count)
                        CountCorrect++;
                    sum1 += expected.Count;
                    sum2 += actual.Count;
                }
            }

            Console.WriteLine($"Процент совпадений решений: {(double)CountCorrect / countK/iterN*100}%");
            Console.WriteLine($"Среднее время выполнения алгоритма FF: {sw.Elapsed.TotalMilliseconds/countK/iterN} ");
            Console.WriteLine($"Среднее отклонение приближенного решения от точного: {((double)(Math.Abs(sum2 - sum1)) /sum1/countK/iterN)}");
        }
        static void Main(string[]argv)
        {

            int n = 8;
            //BigInteger n = 250000;
            Console.WriteLine($"Кол-во предметов: {n}");
            int M = random.Next(1, 10000);
            Console.WriteLine($"Вместимость 1-го контейнера: {M}");
            var weights = new List<Item>();
            for (int i = 0; i < n; i++)
                weights.Add(new Item(i, random.Next(1,2*(M/3))));
            Console.WriteLine();
            Console.WriteLine();



            Stopwatch sw = new Stopwatch(); 
            var min = int.MaxValue;
            List<Conteiner> bestpermutation = null;
            sw.Start();
            foreach (var indexes in GetPermutations(n))
            {
                var currentPermutation = indexes.Select(index => weights[index]).ToList();

                var permff = FF(M, currentPermutation);
                if (permff.Count < min)
                {
                    bestpermutation = permff;
                    min = permff.Count;
                }
            }
            sw.Stop();
            var permutationTIME = sw.Elapsed;
            Console.WriteLine("Переборный.");
            OutputInformationContainers(bestpermutation);


            sw.Restart();
            var conteinersFF = FF(M, weights);
            sw.Stop();
            var FFtime = sw.Elapsed;
            Console.WriteLine("First Fit.");
            OutputInformationContainers(conteinersFF);

            Console.WriteLine();


            sw.Restart();
            var conteinersFFS = FFS(M, weights);
            sw.Stop();
            var FFStime = sw.Elapsed;
            Console.WriteLine("First Fit Decreasing.");
            OutputInformationContainers(conteinersFFS);

            Console.WriteLine();
            Console.WriteLine($"Время выполнения Переборного алгоритма: {permutationTIME}");
            Console.WriteLine();
            Console.WriteLine($"Время выполнения FF: {FFtime}");
            Console.WriteLine();
            Console.WriteLine($"Время выполнения FFS: {FFStime}");

            Console.WriteLine();
            Console.WriteLine("Для продолжения нажмите Enter...");
            Console.ReadLine();
            Console.Clear();
            GC.Collect();GC.Collect();GC.Collect();
            Console.WriteLine("FF и Переборный");
            PerformanceTest(M, FF);
            Console.WriteLine();
            Console.WriteLine("FFS и Переборный");
            PerformanceTest(M, FFS);

            GC.Collect();GC.Collect();GC.Collect();

            Console.WriteLine();
            Console.WriteLine("Худший случай, если каждому предмету нужен отдельный контейнер.");
            var worstarrya = new List<Item>();
            for (int i = 0; i < n; i++)
                worstarrya.Add(new Item(i, M));

            Console.Write("Среднее время выполнения переборного алгоритма: ");
            BestORWorstForPermutation(worstarrya, M);
            Console.WriteLine();
            Console.Write("Среднее время выполнения алгоритма FF: ");
            BestORWorst(M, worstarrya, FF);
            Console.WriteLine();
            Console.Write("Среднее время выполнения алгоритма FFS: ");
            BestORWorst(M, worstarrya, FFS);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Лучший случай, если все предметы вмещаются в один контейнер.");
            var bestarray = new List<Item>();
            for(int i = 0;i<n;i++)
                bestarray.Add(new Item(i, M/n-1));
            Console.Write("Среднее время выполнения переборного алгоритма: ");
            BestORWorstForPermutation(bestarray, M);
            Console.WriteLine();
            Console.Write("Среднее время выполнения алгоритма FF: ");
            BestORWorst(M, bestarray, FF);
            Console.WriteLine();
            Console.Write("Среднее время выполнения алгоритма FFS: ");
            BestORWorst(M, bestarray, FFS);
            Console.WriteLine();



        }
        public static void BestORWorst(int capacity,List<Item>containers, Func<int, List<Item>, List<Conteiner>> act)
        {
            int count = 100;
            Stopwatch sw = new Stopwatch();
            List<Conteiner> res = null;
            for(int i =0;i<count;i++)
            {
                sw.Start();
                res = act(capacity, containers);
                sw.Stop();
            }
            Console.Write(sw.Elapsed.TotalMilliseconds / count);
        }
        public static void BestORWorstForPermutation(List<Item>somearray,int capacity)
        {
            Stopwatch sw = new Stopwatch();
            int count = 100;
            int min = int.MaxValue;
            List<Conteiner> bestpermutation = null;
            for (int i = 0; i < count; i++)
            {
                sw.Start();
                foreach (var indexes in GetPermutations(somearray.Count))
                {
                    var currentPermutation = indexes.Select(index => somearray[index]).ToList();

                    var permff = FF(capacity, currentPermutation);
                    if (permff.Count < min)
                    {
                        bestpermutation = permff;
                        min = permff.Count;
                    }
                }
                sw.Stop();
            }
           
            Console.Write(sw.Elapsed.TotalMilliseconds / count);
        }
    }
}