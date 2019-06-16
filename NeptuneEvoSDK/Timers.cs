using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Redage.SDK
{
    public static class Timers
    {
        public static Dictionary<string, nTimer> timers = new Dictionary<string, nTimer>();
        public static nLog Log = new nLog("nTimer", false);
        private static Config config = new Config("Timers");
        private static Thread thread;

        private static int delay;
        private static int clearDelay;

        public static void Init()
        {
            delay = config.TryGet<int>("delay", 100);
            clearDelay = config.TryGet<int>("clearDelay", 300000);

            thread = new Thread(Logic);
            thread.IsBackground = true;
            thread.Name = "nTimer";
            thread.Start();

            Timers.Start(clearDelay, () =>
            {
                lock (Timers.timers)
                {
                    List<nTimer> timers_ = new List<nTimer>(Timers.timers.Values);
                    foreach (nTimer t in timers_)
                    {
                        if (t.isFinished) Timers.timers.Remove(t.ID);
                    }
                }
            });
        }
        private static void Logic()
        {
            while (true)
            {
                try
                {
                    if (timers.Count < 1) continue;

                    List<nTimer> timers_ = new List<nTimer>(timers.Values);

                    foreach (nTimer timer in timers_)
                    {
                        timer.Elapsed();
                    }
                    Thread.Sleep(delay);

                }
                catch (Exception e)
                {
                    Log.Write($"Timers.Logic: {e.ToString()}", nLog.Type.Error);
                }
            }
        }

        /// <summary>
        /// Находит и возвращает объект таймера
        /// </summary>
        /// <param name="id">Уникальный идентификатор таймера</param>
        /// <returns>Объект таймера</returns>
        public static nTimer Get(string id)
        {
            if (timers.ContainsKey(id))
                return timers[id];
            return null;
        }

        /// <summary>
        /// Start() запускает таймер и возвращает случайный ID
        /// </summary>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string Start(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new nTimer(action, id, interval));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// Start() запускает таймер с уникальным ID
        /// </summary>
        /// <exception>
        /// Exception возникает при передаче уже существующего ID или значения null
        /// </exception>
        /// <param name="id">Уникальный идентификатор таймера</param>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string Start(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new nTimer(action, id, interval));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartOnce() запускает таймер один раз и возвращает случайный ID
        /// </summary>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartOnce(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new nTimer(action, id, interval, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartOnce() запускает таймер один раз и возвращает ID
        /// </summary>
        /// <exception>
        /// Exception возникает при передаче уже существующего ID или значения null
        /// </exception>
        /// <param name="id">Уникальный идентификатор таймера</param>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartOnce(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new nTimer(action, id, interval, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartTask() запускает таймер отдельной задачей и возвращает случайный ID
        /// </summary>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartTask(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new nTimer(action, id, interval, false, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartTask() запускает таймер отдельной задачей и возвращает ID
        /// </summary>
        /// <exception>
        /// Exception возникает при передаче уже существующего ID или значения null
        /// </exception>
        /// <param name="id">Уникальный идентификатор таймера</param>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new nTimer(action, id, interval, false, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartOnceTask() запускает таймер один раз отдельной задачей и возвращает случайный ID
        /// </summary>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartOnceTask(int interval, Action action)
        {
            string id = Guid.NewGuid().ToString();
            try
            {
                timers.Add(id, new nTimer(action, id, interval, true, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }
        /// <summary>
        /// StartOnceTask() запускает таймер один раз отдельной задачей и возвращает ID
        /// </summary>
        /// <exception>
        /// Exception возникает при передаче уже существующего ID или значения null
        /// </exception>
        /// <param name="id">Уникальный идентификатор таймера</param>
        /// <param name="interval">Интервал срабатывания действия</param>
        /// <param name="action">Лямбда-выражение с действием</param>
        /// <returns>Уникальный ID таймера</returns>
        public static string StartOnceTask(string id, int interval, Action action)
        {
            try
            {
                if (timers.ContainsKey(id)) throw new Exception("This id is already in use!");
                if (id is null) throw new Exception("Id cannot be null");

                timers.Add(id, new nTimer(action, id, interval, true, true));
                return id;
            }
            catch (Exception e)
            {
                Log.Write($"Timer.Start.{id}.Error: {e.Message}", nLog.Type.Error);
                return null;
            }
        }

        public static void Stop(string id)
        {
            if (id is null) throw new Exception("Trying to stop timer with NULL ID");
            if (timers.ContainsKey(id))
            {
                timers[id].isFinished = true;
                timers.Remove(id);
            }
        }

        public static void Stats()
        {
            string timers_ = "";
            foreach (nTimer t in timers.Values)
            {
                string state = (t.isFinished) ? "stopped" : "active";
                timers_ += $"{t.ID}:{state} ";
            }

            Log.Write(
                $"\nThread State = {thread.ThreadState.ToString()}" +
                $"\nTimers Count = {timers.Count}" +
                $"\nTimers = {timers_}" +
                $"\n");
        }
    }
    public class nTimer
    {
        public string ID { get; }
        public int MS { get; set; }
        public DateTime Next { get; private set; }

        public Action action { get; set; }

        public bool isOnce { get; set; }
        public bool isTask { get; set; }
        public bool isFinished { get; set; }

        public nTimer(Action action_, string id_, int ms_, bool isonce_ = false, bool istask_ = false)
        {
            action = action_;

            ID = id_;
            MS = ms_;
            Next = DateTime.Now.AddMilliseconds(MS);

            isOnce = isonce_;
            isTask = istask_;
            isFinished = false;
        }

        public void Elapsed()
        {
            try
            {
                if (isFinished) return;

                if (Next <= DateTime.Now)
                {
                    if (isOnce) isFinished = true;
                    Next = DateTime.Now.AddMilliseconds(MS);

                    Timers.Log.Debug($"Timer.Elapsed.{ID}.Invoke");

                    if (isTask) Task.Run(() => action.Invoke());
                    else action.Invoke();

                    Timers.Log.Debug($"Timer.Elapsed.{ID}.Completed", nLog.Type.Success);
                }

            }
            catch (Exception e)
            {
                Timers.Log.Write($"Timer.Elapsed.{ID}.Error: {e.ToString()}", nLog.Type.Error);
            }
        }

    }
}
