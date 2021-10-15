using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var counter = 0;
            var list = new List<Tasking>() {
                new Tasking(1){Work = () => System.Threading.Thread.Sleep(100) },
                new Tasking(2),
                new Tasking(3),
             };
            MyQueue queue = new MyQueue();
            var taskList = new List<Task>();
            foreach (var task in list)
            {
                var sem = queue.GetToken();
                try
                {
                    counter = Interlocked.Increment(ref counter);
                    var hego = Task.Run(() =>
                    {
                        task.Work();
                        queue.Enqueue(task.Conter, sem);
                        System.Diagnostics.Debug.WriteLine($"add:{task.Conter}");
                    });
                    taskList.Add(hego);
                }
                catch
                {
                    sem.Release();
                }
            }
            while (queue.Count != list.Count)
            {
                System.Threading.Thread.Sleep(100);
            }
            var flg = false;
            while (true)
            {
                var val = queue.Dequeue();
                System.Diagnostics.Debug.WriteLine(val);
                Console.WriteLine(val);
                if (flg)
                {
                    break;
                }
                flg = taskList.All(x => x.IsCompleted);
            }
            System.Diagnostics.Debug.WriteLine($"counter:{counter}");
        }
    }
    class MyQueue
    {
        Queue<int> queue = new Queue<int>();
        Queue<SemaphoreSlim> processToken = new Queue<SemaphoreSlim>();

        public SemaphoreSlim GetToken()
        {
            var obj = new SemaphoreSlim(1);
            lock (processToken)
            {
                processToken.Enqueue(obj);
                Task.Run(async () =>
                {
                    await obj.WaitAsync().ConfigureAwait(false);
                    var token = processToken.Peek();
                    token.Release();

                }).Wait();
            }
            return obj;
        }
        public void Enqueue(int val, SemaphoreSlim sem)
        {
            Task.Run(async () =>
            {

                await sem.WaitAsync().ConfigureAwait(false);
                lock (queue)
                {
                    queue.Enqueue(val);
                }
                lock (processToken)
                {

                    processToken.Dequeue();
                    if (processToken.Count != 0)
                    {
                        processToken.Peek().Release();
                    }
                }
            });
        }

        public int Dequeue()
        {
            lock (queue)
            {
                return queue.Dequeue();
            }
        }

        public int Count
        {
            get
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }
    }
    class Tasking
    {
        public Tasking(int a)
        {
            this.Conter = a;
        }
        public int Conter { get; set; }
        public Action Work { get; set; } = () => { System.Threading.Thread.Sleep(10); };
    }
}
