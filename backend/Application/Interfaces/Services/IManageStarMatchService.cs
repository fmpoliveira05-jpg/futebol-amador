using Application.DTOs.Hub;

namespace Application.Interfaces.Services
{
    public interface IManageStarMatchService
    {
        public Task AddAdminHub(AdminsJoinMatchDTO admin);
        string GetConnectionStringAdmin(Guid idAdmin);
        public Task<bool> RemoveAdminHub(string connectionId);
        public Task<bool> StartMatch(Guid IdAdmin);
        public int GetCountAdminsInHub();
    }
}
