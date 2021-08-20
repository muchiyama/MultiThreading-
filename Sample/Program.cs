using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.MyTaskFamily
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            
            Console.WriteLine($"検証内容を選んでください{Environment.NewLine}{    "1: SetMinThreadsを変更した場合の検証"}{Environment.NewLine}{    "2: 別クラスにてマルチスレッド処理を実行した場合の検証"}");
            var supportedInputsForStrategy = new List<string>{ "1", "2",};

            var mode = AskReadKeyWhileValidInputIsPressed(
                (input, supportedInputs) => { return supportedInputs.Contains(input); }
                , supportedInputsForStrategy);

            Console.WriteLine($"非同期のタイプを選んでください{Environment.NewLine}{    "1: Task"}{Environment.NewLine}{    "2: Parallel"}");
            var supportedInputsForProsessor = new List<string> { "1", "2", };

            var inputForProcessor = AskReadKeyWhileValidInputIsPressed(
                (input, supportedInputs) => { return supportedInputs.Contains(input); }
                , supportedInputsForProsessor);

            IAsyncProcessor processor;
            if (inputForProcessor == "1")
                processor = new TaskAsyncProcessor();
            else
                processor = new ParallecAsyncProcessor();

            ISmapleStrategy strategy;
            if (mode == "1")
                strategy = new SetMinThreadsを変更した場合の検証(processor);
            else
            {
                IAsyncProcessor additionalprocessor = processor.Clone();
                strategy = new 別クラスにてマルチスレッド処理を実行した場合の検証(processor, additionalprocessor);
            }

            await strategy.Execute();

            Console.WriteLine("全ての処理が完了しました。終了するには何かキーを押してください。");
            Console.ReadLine();
        }

        public static void LoggingHeader()
            => Console.WriteLine($"スレッドID,開始/終了,経過時間,GUID,ワーカースレッドプール数,IOスレッドプール数,追加メッセージ");

        public static void LoggingConsole(int ManagedThreadId, string startOrCompleted, TimeSpan elapsed, Guid id, int workerThreadCount, int completionThreadCount, string additionalMessage = "")
        {
            if(string.IsNullOrEmpty(additionalMessage))
                Console.WriteLine($"[{ManagedThreadId}],{startOrCompleted},{elapsed},{id},{workerThreadCount},{completionThreadCount}");
            else
                Console.WriteLine($"[{ManagedThreadId}],{startOrCompleted},{elapsed},{id},{workerThreadCount},{completionThreadCount},{additionalMessage}");
        }

        public static void Wait(int secound)
        {
            var limit = new TimeSpan(0, 0, 0, secound);
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            while(stopWatch.Elapsed < limit)
            {
                
            }
        }

        public static string AskReadKeyWhileValidInputIsPressed(Func<string, List<string>, bool> match, List<string> supportedInputs)
        {
            //Console.WriteLine("以下の範囲から入力をしてください。");
            //Console.WriteLine($"    {string.Join(",", supportedInputs)}");

            var input = string.Empty;
            while (true)
            {
                input = Console.ReadLine();
                var result = match(input, supportedInputs);
                if (result) break;
                else Console.WriteLine("入力値が範囲外の為もう一度入力をしてください");
            }

            Console.WriteLine(Environment.NewLine);

            return input;
        }

        public static List<int> Iterator()
        {
            var iterater = new List<int>();
            for (var i = 0; i < 10000; i++)
                iterater.Add(i);
            return iterater;
        }

        public static void DiagnoticLogging(System.Diagnostics.Stopwatch stopWatch, string additionalMessage = "")
        {
            var id = Guid.NewGuid();
            System.Threading.ThreadPool.GetAvailableThreads(out var worker, out var completion);
            LoggingConsole(Thread.CurrentThread.ManagedThreadId, "開始", stopWatch.Elapsed, id, worker, completion, additionalMessage);
            Wait(10);
            LoggingConsole(Thread.CurrentThread.ManagedThreadId, "終了", stopWatch.Elapsed, id, worker, completion, additionalMessage);
        }

        public static async Task WaitStartProcces()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            var separator = new TimeSpan(0, 0, 0, 1, 500);

            while(stopWatch.Elapsed < separator)
            {
                Console.WriteLine("waiting...");
                await Task.Delay(500);
            }

            Console.WriteLine($"検証を開始します...{Environment.NewLine}");
        }

        public static void SetMinMultiThreadContext()
        {
            Console.WriteLine($"最小マルチスレッド数の設定をしますか？{Environment.NewLine}{    "1: はい"}{Environment.NewLine}{    "2: いいえ"}");
            var supportedInputsForSetMinThreadPoolIfSetParameters = new List<string> { "1", "2" };
            var isSettingMinThreads = AskReadKeyWhileValidInputIsPressed(
                (input, supportedInputs) => { return supportedInputs.Contains(input); }
                , supportedInputsForSetMinThreadPoolIfSetParameters);
            if (isSettingMinThreads == "2") return;


            Console.WriteLine($"1～10の中から最小スレッド数を選んでください");
            var supportedInputsForSetMinThreadPool = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            Console.WriteLine($"    → Workerスレッド数を選んでください");
            var workerMinThreadValue = AskReadKeyWhileValidInputIsPressed(
                (input, supportedInputs) => { return supportedInputs.Contains(input); }
                , supportedInputsForSetMinThreadPool);
            Console.WriteLine($"    → IOスレッド数を選んでください");
            var completionMinThreadValue = AskReadKeyWhileValidInputIsPressed(
                (input, supportedInputs) => { return supportedInputs.Contains(input); }
                , supportedInputsForSetMinThreadPool);

            ThreadPool.SetMinThreads(int.Parse(workerMinThreadValue), int.Parse(completionMinThreadValue));
        }

        public interface ISmapleStrategy
        {
            Task Execute();
        }

        public class SetMinThreadsを変更した場合の検証 : ISmapleStrategy
        {
            IAsyncProcessor m_processor;
            public SetMinThreadsを変更した場合の検証(IAsyncProcessor processor)
            {
                m_processor = processor;
            }

            public async Task Execute()
            {
                SetMinMultiThreadContext();

                await WaitStartProcces();

                m_processor.Procces(DiagnoticLogging, Iterator());

                Console.WriteLine("全ての処理が完了しました。終了するには何かキーを押してください。");
                Console.ReadLine();
            }
        }

        public class 別クラスにてマルチスレッド処理を実行した場合の検証 : ISmapleStrategy
        {
            IAsyncProcessor m_processorA;
            IAsyncProcessor m_processorB;

            public 別クラスにてマルチスレッド処理を実行した場合の検証(IAsyncProcessor processorA, IAsyncProcessor processorB)
            {
                m_processorA = processorA;
                m_processorB = processorB;
            }

            public async Task Execute()
            {
                SetMinMultiThreadContext();
                await WaitStartProcces();

                List<Task> tasks = new List<Task>();
                tasks.Add(new Task(() => m_processorA.Procces(DiagnoticLogging, Iterator(), "classA")));
                tasks.Add(new Task(() => m_processorB.Procces(DiagnoticLogging, Iterator(), "classB")));
                tasks.ForEach(f => f.Start());
                Task.WaitAll(tasks.ToArray());

                Console.WriteLine("全ての処理が完了しました。終了するには何かキーを押してください。");
                Console.ReadLine();
            }
        }

        public interface IAsyncProcessor
        {
            void Procces(Action<Stopwatch, string> action, List<int> iterator, string additionalMessage = "");
            IAsyncProcessor Clone();
        }

        public class ParallecAsyncProcessor : IAsyncProcessor
        {
            public IAsyncProcessor Clone()
                => new ParallecAsyncProcessor();

            public void Procces(Action<Stopwatch, string> action, List<int> iterator, string additionalMessage = "")
            {
                LoggingHeader();

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Parallel.ForEach(iterator, i =>
                {
                    action(stopWatch, additionalMessage);
                });
            }
        }

        public class TaskAsyncProcessor : IAsyncProcessor
        {
            public IAsyncProcessor Clone()
                => new TaskAsyncProcessor();

            public void Procces(Action<Stopwatch, string> action, List<int> iterator, string additionalMessage = "")
            {
                LoggingHeader();

                var stopWatch = new Stopwatch();
                var tasks = new List<Task>();
                iterator.ForEach(f =>
                {
                    var task = new Task(() => action(stopWatch, additionalMessage));
                    tasks.Add(task);
                });

                stopWatch.Start();
                tasks.ForEach(f => f.Start());
                Task.WaitAll(tasks.ToArray());
            }
        }

        public interface IWaitHandler
        {
            void Wait();
        }
    }
}
