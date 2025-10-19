using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IMatchService
    {
        public Task<Matches> PostPoneMatch(PostponeMatchDTO dto);
        public Task<Matches> AcceptPostPoneMatch(AcceptRefusePostPoneDTO dto);
        public Task<Matches> RejectPostPoneMatch(AcceptRefusePostPoneDTO dto);
    }
}
