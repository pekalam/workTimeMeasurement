﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Domain.Repositories;
using Domain.WorkTime.Events;
using ReflectionMagic;

[assembly: InternalsVisibleTo("Domain.UnitTests")]
namespace Domain.WorkTime
{
    public class WorkTimeBuildService
    {
        private readonly IWorkTimeEsRepository _repository;

        public WorkTimeBuildService(IWorkTimeEsRepository repository)
        {
            _repository = repository;
        }

    }

    public class WorkTimeSnapshot
    {
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public User.User User { get; set; }
        public bool AutoStart { get; set; }
    }

    public class WorkTime
    {
        private readonly List<Event> _pendingEvents = new List<Event>();
        private readonly List<Event> _actionEvents = new List<Event>();

        internal WorkTime(int aggregateId, User.User user, DateTime? startDate, DateTime endDate)
        {
            AggregateId = aggregateId;
            Create(user, startDate, endDate);
        }

        private WorkTime() { }


        public long AggregateVersion { get; private set; }
        public int AggregateId { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime DateCreated { get; private set; }
        public User.User User { get; private set; }
        public bool AutoStart { get; private set; }
        public bool Started => StartDate.HasValue;
        public bool FromSnapshot { get; private set; }

        public IReadOnlyList<Event> PendingEvents => _pendingEvents;
        public IReadOnlyList<Event> ActionEvents => _actionEvents;

        public void MarkPendingEventsAsHandled() => _pendingEvents.Clear();

        private void AddEvent(Event ev)
        {
            ev.AggregateVersion = ++AggregateVersion;
            _pendingEvents.Add(ev);
        }


        private void Create(User.User user, DateTime? startDate, DateTime endDate)
        {
            StartDate = startDate;
            AutoStart = startDate.HasValue;
            User = user;
            EndDate = endDate;
            DateCreated = DateTime.UtcNow;
            AddEvent(new WorkTimeCreated(AggregateId, DateTime.UtcNow, StartDate, EndDate,DateCreated, User, AutoStart));
        }

        private void CheckIsStarted()
        {
            if (!StartDate.HasValue)
            {
                throw new Exception("WorkTime not started");
            }
        }

        private void StartWorkTime()
        {
            StartDate = DateTime.UtcNow;
            AddEvent(new WorkTimeStarted(AggregateId, DateTime.UtcNow, StartDate.Value));
        }

        public void StartManually()
        {
            if (AutoStart)
            {
                throw new Exception("Work time should be automatically started");
            }
            StartWorkTime();
        }

        public void Start()
        {
            if (!AutoStart)
            {
                throw new Exception("Work time not set to autoStart");
            }
            StartWorkTime();
        }

        public void AddMouseAction()
        {
            CheckIsStarted();

            var ev = new MouseAction(AggregateId, DateTime.UtcNow, 0);
            _actionEvents.Add(ev);
            AddEvent(ev);
        }

        public void AddKeyboardAction()
        {
            CheckIsStarted();

            var ev = new KeyboardAction(AggregateId, DateTime.UtcNow, 0);
            _actionEvents.Add(ev);
            AddEvent(ev);
        }

        public WorkTimeSnapshotCreated TakeSnapshot()
        {
            CheckIsStarted();

            _actionEvents.Clear();
            var snapshot = new WorkTimeSnapshot()
            {
                StartDate = StartDate,
                User = User,
                EndDate = EndDate,
                AutoStart = AutoStart,
                DateCreated = DateCreated,
            };

            var ev = new WorkTimeSnapshotCreated(AggregateId, DateTime.UtcNow, snapshot);
            AddEvent(ev);
            return ev;
        }

        public void RollbackToSnapshot(WorkTimeSnapshotCreated workTimeSnapshotEvent)
        {
            CheckIsStarted();
            if (workTimeSnapshotEvent.AggregateId != AggregateId)
            {
                throw new Exception("Invalid snapshot aggregateId");
            }

            AggregateVersion = workTimeSnapshotEvent.AggregateVersion;
            FromSnapshot = true;
            _pendingEvents.Clear();
            _actionEvents.Clear();

            Apply(workTimeSnapshotEvent);
        }

        private void Apply(WorkTimeSnapshotCreated snapshotCreated)
        {
            var snap = snapshotCreated.Snapshot;
            StartDate = snap.StartDate;
            User = snap.User;
            EndDate = snap.EndDate;
            AutoStart = snap.AutoStart;
            DateCreated = snap.DateCreated;
        }

        private void Apply(KeyboardAction keyboardAction)
        {
            _actionEvents.Add(keyboardAction);
        }

        private void Apply(MouseAction mouseAction)
        {
            _actionEvents.Add(mouseAction);
        }

        private void Apply(WorkTimeStarted workTimeStarted)
        {
            StartDate = workTimeStarted.StartDate;
        } 

        private void Apply(WorkTimeCreated workTimeCreated)
        {
            StartDate = workTimeCreated.StartDate;
            EndDate = workTimeCreated.EndDate;
            DateCreated = workTimeCreated.DateCreated;
            User = workTimeCreated.User;
            AutoStart = workTimeCreated.AutoStart;
        }

        public static WorkTime CreateFromSnapshot(WorkTimeSnapshotCreated snapshotCreated)
        {
            var workTime = new WorkTime();
            workTime.AggregateId = snapshotCreated.AggregateId;
            workTime.AggregateVersion = snapshotCreated.AggregateVersion;
            workTime.FromSnapshot = true;
            workTime.Apply(snapshotCreated);
            return workTime;
        }

        public static WorkTime FromEvents(IEnumerable<Event> events)
        {
            var workTime = new WorkTime();
            Event last = null;
            foreach (var ev in events)
            {
                if (last == null)
                {
                    if (ev.AggregateVersion != 1)
                    {
                        throw new Exception($"Cannot create aggregate starting from event version {ev.AggregateVersion}");
                    }

                    workTime.AggregateId = ev.AggregateId;
                }

                workTime.AsDynamic().Apply(ev);
                last = ev;
            }

            if (last == null)
            {
                throw new Exception("empty event list");
            }

            workTime.AggregateVersion = last.AggregateVersion;

            return workTime;
        }
    }


    //
    // public class NoAction : Event { }
    //
    // public class Away : Event { }


}