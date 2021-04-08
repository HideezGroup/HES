using System;
using System.Timers;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class CountdownTimer : IDisposable
    {
        private Timer _timer;
        private int _timeout;
        private int _countdownTotal;
        private int _percentComplete;

        public Action<int> OnTick;
        public Func<Task> OnElapsed;

        public CountdownTimer(int timeout)
        {
            _countdownTotal = timeout;
            _timeout = (_countdownTotal * 1000) / 100;
            _percentComplete = 0;
            SetupTimer();
        }

        public void SetupTimer()
        {
            _timer = new Timer(_timeout);
            _timer.Elapsed += HandleTick;
            _timer.AutoReset = false;
        }

        public void Start()
        {
            _timer.Start();
        }

        private void HandleTick(object sender, ElapsedEventArgs args)
        {
            _percentComplete++;
            OnTick?.Invoke(_percentComplete);

            if (_percentComplete == 100)
            {
                OnElapsed?.Invoke();
            }
            else
            {
                SetupTimer();
                Start();
            }
        }


        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
