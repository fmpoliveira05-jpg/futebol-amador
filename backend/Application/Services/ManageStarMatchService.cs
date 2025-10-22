/*
 using Application.DTOs.Hub;
using Application.Interfaces.Repositorys;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Enums;

namespace Application.Services
{
    public class ManageStarMatchService: IManageStarMatchService
    {
        private readonly IMatchRepository matchRepository;
        private readonly IUnityOfWork unityOfWork;
        
        public ManageStarMatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
        }

        private string validateStartMatch(Matches match)
        {
            if (match == null)
            {
                return "Não dá para aceitar matchs que não estão agendadas";
            }

            if (match.MatchDate < DateTime.UtcNow)
            {
                return "Não dá para iniciar a match enquanto não passar da data e hora da mesma";
            }

            return "";
        }

        public async Task ValidateAdmin(Guid idAdmin)
        {

        }
   
        public async Task AddAdminHub(AdminsJoinMatchDTO admin)
        {
            int numAdmins = admins.Count;
            if (numAdmins >= 2)
            {
                throw new InvalidOperationException("Já existem 2 admins no hub.");
            }

            var match = await matchRepository.GetScheduledMatchById(admin.idMatch);

            var validateMatch = validateStartMatch(match);
            if (validateMatch != null) {
                throw new BusinessRuleException(validateMatch);
            }

            if (numAdmins > 0)
            {
                AdminsJoinMatchDTO haveAdminTeam = admins.Values.FirstOrDefault(a => a.IdTeam == admin.IdTeam);
                
                if (haveAdminTeam != null)
                {
                    throw new InvalidOperationException("Já existe um admin dessa equipa a iniciar partida");
                }
            }

            admins.Add(admin.IdAdmin, admin);
        }

        public string GetConnectionStringAdmin(Guid idAdmin)
        {
            if (admins.Count == 0)
            {
                throw new EmptyCollectionException("Ainda não existem admins no HUB");
            }

            var admin = admins.Values.FirstOrDefault(a => a.IdAdmin == idAdmin);

            if (admin == null)
            {
                throw new ArgumentNullException("O id de admin não coreesponde ao id de nenhum admin no hub");
            }

            return admin.ConnectionId;
        }

        public Task<bool> RemoveAdminHub(string connectionId)
        {
            AdminsJoinMatchDTO admin = admins.Values.FirstOrDefault(a => a.ConnectionId == connectionId);

            if (admin == null)
            {
                throw new InvalidOperationException("Já existe um admin dessa equipa a iniciar partida");
            }

            admins.Remove(admin.IdAdmin);
            return Task.FromResult(true);
        }

        //Metodo para começar a partida com os dois
        public async Task<bool> StartMatch(Guid idAdmin)
        {
            if (admins.Count() < 2)
            {
                throw new InvalidOperationException("As duas equipas ainda não aprovaram a match");
            }

            var matchAdmin = admins.Values.FirstOrDefault(a => a.IdAdmin == idAdmin);
         
            if (matchAdmin == null)
            {
                throw new InvalidOperationException("Algum dos admins está nulo");
            }

            var match = await matchRepository.GetScheduledMatchById(matchAdmin.idMatch);

            var validateMatch = validateStartMatch(match);
            if (validateMatch != null)
            {
                throw new BusinessRuleException(validateMatch);
            }

            match.MatchStatus = MatchStatus.IN_PROGRESS;
            await unityOfWork.SaveChangesAsync();

            return await Task.FromResult(true);
        }

        public int GetCountAdminsInHub()
        {
            return admins.Count();
        }
    }
}

 
 */