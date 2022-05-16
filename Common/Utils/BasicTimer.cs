using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace Utils
{
    public class BasicTimer
    {
        private readonly Queue<Action> extra;

        private readonly Timer timer;

        private Action callback;

        private TimeSpan remaining = TimeSpan.Zero;

        public BasicTimer(Action callback = null, TimeSpan time = default, bool inert = false, params Action[] extra)
        {
            this.callback = callback;
            Time = time;
            Inert = inert;
            this.extra = new Queue<Action>(extra);
            timer = new Timer() { AutoReset = false };
            timer.Elapsed += Timer_Elapsed;
        }

        public DateTime? StartTime { get; private set; }

        public DateTime? ResumeTime { get; private set; }

        public ReadOnlyCollection<Action> Extra => new ReadOnlyCollection<Action>(extra.ToList());

        public TimeSpan Time { get; set; }

        public bool Inert { get; }

        public BasicTimer Frozen => new BasicTimer(callback, Time, true);

        public DateTime? End => ResumeTime is null ? null : ResumeTime + remaining;

        public TimeSpan? Remaining => End - DateTime.UtcNow;

        public bool IsTicking { get; private set; }

        public BasicTimer SkipTo(TimeSpan remaining)
        {
            if ((this.remaining = remaining) > TimeSpan.Zero)
            {
                ResumeTime = DateTime.UtcNow;
                if (!Inert) ChangeTimer(this.remaining);
            }
            else
            {
                ChangeTimer(TimeSpan.Zero);
                Wrap();
            }

            return this;
        }

        public BasicTimer SetCallback(Action callback)
        {
            this.callback = callback;
            return this;
        }

        public BasicTimer Clear()
        {
            callback = null;
            return this;
        }

        public BasicTimer ContinueWith(Action callback)
        {
            if (IsTicking)
            {
                extra.Enqueue(callback);
            }
            else
            {
                callback();
            }

            return this;
        }

        public BasicTimer Resume()
        {
            if (!IsTicking && remaining > TimeSpan.Zero)
            {
                ResumeTime = DateTime.UtcNow;
                if (!Inert) ChangeTimer(remaining);
            }

            return this;
        }

        public BasicTimer Pause()
        {
            if (IsTicking && remaining > TimeSpan.Zero && ResumeTime is DateTime dateTime)
            {
                remaining -= DateTime.UtcNow - dateTime;
                timer.Stop();
                IsTicking = false;
                return this;
            }

            return null;
        }

        public BasicTimer Stop()
        {
            if (IsTicking)
            {
                timer.Stop();
                IsTicking = false;
                remaining = TimeSpan.Zero;
                return this;
            }

            return null;
        }

        public BasicTimer Start(TimeSpan? time = null)
        {
            if (callback is null)
            {
                throw new NotImplementedException();
            }

            if (!IsTicking)
            {
                if (time is TimeSpan t1)
                {
                    Time = t1;
                }

                if (Time is TimeSpan t2)
                {
                    StartTime = ResumeTime = DateTime.UtcNow;
                    remaining = t2;

                    if (!Inert)
                    {
                        ChangeTimer(remaining);
                    }
                }
            }

            return this;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) => Wrap(e.SignalTime);

        private void Wrap(object state = null)
        {
            IsTicking = false;
            remaining = TimeSpan.Zero;
            callback?.Invoke();

            while (extra.Count > 0)
            {
                extra.Dequeue()();
            }
        }

        private void ChangeTimer(TimeSpan dueTime)
        {
            timer.Interval = dueTime.TotalMilliseconds;

            if (dueTime > TimeSpan.Zero)
            {
                IsTicking = true;
                timer.Start();
            }
        }
    }
}