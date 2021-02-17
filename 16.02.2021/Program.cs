using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace _16._02._2021
{
    public class Program
    {
        public static class Falling
        {
            public const int Width = 120;
            public const int Height = 30;
            public const int Lines = 50;
            public const int StartDelay = 10;

            public static async Task Start()
            {
                var tasks = new List<Task>();
                for (var a = 0; a < Lines; a++)
                {
                    var task = Task.Run(ContuorLine);
                    tasks.Add(task);
                    await Task.Delay(StartDelay);
                }
            }

            private static async Task ContuorLine()
            {
                while (true)
                {
                    var column = RandomHelper.Rand(0, Width);


                    await FallingLine.StartNew(column);
                }
            }
        }

        public class FallingLine
        {
            private int MinLength = 4;
            private int MaxLength = 18;
            private int MinUpdateTime = 20;
            private int MaxUpdateTime = 50;

            private string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXUZ*/+-=?~@#$%";
            private int Column;

            private int Length;
            private int UpdateTime;
            private char previous1 = ' ';
            private char previous2 = ' ';
            private int Row;

            private FallingLine(int column)
            {
                Length = RandomHelper.Rand(MinLength, MaxLength + 1);
                UpdateTime = RandomHelper.Rand(MinUpdateTime, MaxUpdateTime + 1);
                Column = column;
            }

            public static async Task StartNew(int column)
            {
                var n = new FallingLine(column);
                await n.Start();
            }

            private async Task Start()
            {
                for (var a = 0; a < Falling.Height + Length; a++)
                {
                    Step();
                    await Task.Delay(UpdateTime);
                }
            }

            private static bool Range(int row)
            {
                return row > 0 && row < Falling.Height;
            }

            private void Step()
            {
                if (Range(Row))
                {
                    var symbol = Symbols[RandomHelper.Rand(0, Symbols.Length)];
                    ConsoleHelper.Display(new ConsoleTask(Column, Row, symbol, ConsoleColor.White));
                    previous1 = symbol;
                }

                if (Range(Row - 1))
                {
                    ConsoleHelper.Display(new ConsoleTask(Column, Row - 1, previous1, ConsoleColor.Green));
                    previous2 = previous1;
                }

                if (Range(Row - 2))
                {
                    ConsoleHelper.Display(new ConsoleTask(Column, Row - 2, previous2, ConsoleColor.DarkGreen));
                }

                if (Range(Row - Length))
                {
                    ConsoleHelper.Display(new ConsoleTask(Column, Row - Length, ' ', ConsoleColor.Black));
                }
                Row++;
            }
        }

        public class ConsoleTask
        {
            public readonly ConsoleColor Color;
            public readonly int Column;
            public readonly int Row;
            public readonly char Symbol;

            public ConsoleTask(int column, int row, char symbol, ConsoleColor color)
            {
                Color = color;
                Column = column;
                Row = row;
                Symbol = symbol;
            }
        }

        public static class ConsoleHelper
        {
            private static  ConcurrentQueue<ConsoleTask> Order = new ConcurrentQueue<ConsoleTask>();
            private static bool inProcess;

            public static void Display(ConsoleTask task)
            {
                Order.Enqueue(task);
                DisplayCore();
            }

            private static void DisplayCore()
            {
                while (true)
                {
                    if (inProcess)
                    {
                        return;
                    }

                    lock (Order)
                    {
                        if (inProcess)
                        {
                            return;
                        }
                        inProcess = true;
                    }

                    while (Order.TryDequeue(out var task))
                    {
                        Console.SetCursorPosition(task.Column, task.Row);
                        Console.ForegroundColor = task.Color;
                        Console.Write(task.Symbol);
                    }

                    lock (Order)
                    {
                        inProcess = false;
                        if (!Order.IsEmpty)
                        {
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        public static class RandomHelper
        {
            private static int goOff = Environment.TickCount;

            private static readonly ThreadLocal<Random> Random =
                    new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref goOff)));

            public static int Rand(int min, int max)
            {
                return Random.Value.Next(min, max);
            }
        }
        public static void Main()
        {
            var task = Task.Run(Falling.Start);
            Console.CursorVisible = false;
            Console.ReadKey();
        }
    }
}
