using Domain.Entities;

namespace Application.Interfaces.Entities
{
    public interface ICalendar
    {
        public void ScheduledMatch(Matches match);

        public Matches PostPoneMatch(Guid idMatch, DateTime newDate);

        public Matches AcceptPostPoneMatch(Guid idMatch);

        public Matches CancelMatch(Guid idMatch);
    }
}
