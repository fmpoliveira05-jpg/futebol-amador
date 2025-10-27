using System.ComponentModel.DataAnnotations;
using Domain.Constants;
using Domain.Enums;


/***
 * Entidade que representa um jogador no sistema.
 */
namespace Domain.Entities
{
    public class Player : Users
    {
        public Position Position { get; set; }

        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight, ErrorMessage = "Um jogador deve ter entre {0} e {1} centimetors")]
        public int Height { get; set; }

        public Teams? Team { get; set; }

        public Guid? IdTeam { get; set; } //FK

        public bool IsAdmin { get; set; }

        public DateTime? IsAdminLastChangedAt { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O número de convites tem de ser pelo menos 0")]
        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        // EF
        public Player() { }

        public Player(string name, DateOnly dateOfBirth, string address, string email, string password, string phoneNumber, Position position, int height)
            : base(name, dateOfBirth, address, email, password, phoneNumber)
        {
            Position = position;
            Height = height;
            IsAdmin = false;
        }

        /***
         * Metodo que permite adicionar um pedido de adesão a uma equipa
         * 
         * membershipRequests: O pedido de adesão a adicionar
         * 
         * Retorna o pedido de adesão adicionado ou null se não for possível adicionar
         */
        public MembershipRequests AddMembershipRequest(MembershipRequests membershipRequest)
        {
            var existingRequest = MembershipRequests.FirstOrDefault(mr => mr.Id == membershipRequest.Id);
            if (existingRequest != null)
            {
                return null;
            }

            MembershipRequests.Add(membershipRequest);

            return membershipRequest;
        }

        /***
         *  Metodo que permite remover um pedido de adesão a uma equipa
         *  
         *  membershipRequests: O pedido de adesão a remover
         *  
         *  Retorna o pedido de adesão removido ou null se não for possível remover
         */
        public MembershipRequests RemoveMembershipRequest(MembershipRequests membershipRequest)
        {
            var existingRequest = MembershipRequests.FirstOrDefault(mr => mr.Id == membershipRequest.Id);
            if (existingRequest == null)
            {
                return null;
            }

            MembershipRequests.Remove(existingRequest);

            return existingRequest;
        }

        /***
         * Metodo que permite obter um pedido de adesão a uma equipa pelo seu id
         * 
         * id: O id do pedido de adesão a obter
         * 
         * Retorna o pedido de adesão com o id especificado ou null se não for encontrado
         */
        public MembershipRequests GetMembershipRequestById(Guid id)
        {
            return MembershipRequests.FirstOrDefault(mr => mr.Id == id);
        }

        public override string ToString()
        {
            return base.ToString() + $", Position: {Position}, Height: {Height}cm, Team: {(Team != null ? Team.Name : "No Team")}, IsAdmin: {IsAdmin}";
        }
    }
}
