﻿using System;

namespace Domain.WorkTime.Events
{
    public class WorkTimeStarted : Event
    {
        public WorkTimeStarted(int aggregateId, DateTime date, DateTime startDate) : base(aggregateId, date, EventName.WorkTimeStarted)
        {
            StartDate = startDate;
        }

        public DateTime StartDate { get; }
    }
}