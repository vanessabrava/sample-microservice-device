using System;
using System.Threading.Tasks;
using System.Timers;
using static Sample.Microservice.Device.Infra.CrossCutting.Background.Delegates;

namespace Sample.Microservice.Device.Infra.CrossCutting.Background
{
    public class ExecuteAsync
    {
        private Func<Task> ActionAsync { get; set; } = null;
        private Action Action { get; set; } = null;
        private ActionDelegate ActionDelegate { get; set; } = null;
        private Timer Timer { get; set; } = new Timer();
        private int CurrentExecution { get; set; } = 0;
        private int RepeatQuantity { get; set; }

        private ExecuteAsync(int repeatQuantity, int interval)
        {
            RepeatQuantity = repeatQuantity;
            Timer.Interval = interval;
            Timer.Elapsed += new ElapsedEventHandler(OnTimeElapsed);
        }

        public ExecuteAsync(int repeatQuantity, int interval, Action action) : this(repeatQuantity, interval) => Action = action;

        public ExecuteAsync(int repeatQuantity, int interval, Func<Task> actionAsync)
        {
            ActionAsync = actionAsync;
            RepeatQuantity = repeatQuantity;
            Timer.Interval = interval;
            Timer.Elapsed += new ElapsedEventHandler(OnTimeElapsedAsync);
        }

        public ExecuteAsync(int repeatQuantity, int interval, ActionDelegate actionDelegate) : this(repeatQuantity, interval) => ActionDelegate = actionDelegate;

        public bool Stop { get; set; } = false;

        public void Do(bool startsExecutingAction = false)
        {
            if (!startsExecutingAction)
            {
                Timer.Start();
            }
            else if (Action != null || ActionDelegate != null)
            {
                OnTimeElapsed(null, null);
            }
            else if (ActionAsync != null)
            {
                OnTimeElapsedAsync(null, null);
            }
        }

        private async void OnTimeElapsedAsync(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (Stop)
                {
                    Timer.Stop();
                    Timer.Close();
                    Timer.Dispose();
                }
                else
                {
                    Timer.Stop();
                    if (ActionAsync != null) await ActionAsync();
                }
            }
            catch
            {

            }
            finally
            {
                if (Stop)
                {
                    Timer.Close();
                    Timer.Dispose();
                }
                else if (RepeatQuantity > 0)
                {
                    if (RepeatQuantity - 1 > CurrentExecution++)
                    {
                        Timer.Start();
                    }
                    else
                    {
                        Timer.Close();
                        Timer.Dispose();
                    }
                }
                else
                {
                    Timer.Start();
                }
            }
        }

        private void OnTimeElapsed(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (Stop)
                {
                    Timer.Stop();
                    Timer.Close();
                    Timer.Dispose();
                }
                else
                {
                    Timer.Stop();
                    if (Action != null) Action.Invoke();
                    if (ActionDelegate != null)
                    {
                        bool stop = Stop;
                        ActionDelegate.Invoke(CurrentExecution, ref stop);
                        Stop = stop;
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (Stop)
                {
                    Timer.Close();
                    Timer.Dispose();
                }
                else if (RepeatQuantity > 0)
                {
                    if (RepeatQuantity - 1 > CurrentExecution++)
                    {
                        Timer.Start();
                    }
                    else
                    {
                        Timer.Close();
                        Timer.Dispose();
                    }
                }
                else
                {
                    Timer.Start();
                }
            }
        }
    }
}