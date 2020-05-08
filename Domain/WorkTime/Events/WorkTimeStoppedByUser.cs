﻿using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeStoppedByUser : Event
    {
        public WorkTimeStoppedByUser(long aggregateId, DateTime date) : base(aggregateId, date, EventName.StoppedByUser)
        {
        }
    }
}
