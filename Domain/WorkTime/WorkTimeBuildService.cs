using System;
using Domain.Repositories;
using Domain.Services;

namespace Domain.WorkTimeAggregate
{
    public class WorkTimeBuildService
    {
        private readonly IWorkTimeEsRepository _repository;
        private readonly IWorkTimeIdGeneratorService _idGenerator;

        public WorkTimeBuildService(IWorkTimeEsRepository repository, IWorkTimeIdGeneratorService idGenerator)
        {
            _repository = repository;
            _idGenerator = idGenerator;
        }

        public WorkTime CreateStartedManually(User.User user, DateTime endDate, bool start = false)
        {
            if (endDate <= InternalTimeService.GetCurrentDateTime())
            {
                throw new ArgumentException("Invalid end date");
            }

            var id = _idGenerator.GenerateId();

            //todo long
            var workTime = new WorkTime(id, user, null, endDate);
            if (start)
            {
                workTime.StartManually();
            }
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();
            return workTime;
        }

    }
}