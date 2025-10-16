using System.ComponentModel.DataAnnotations;
using Domain.Enums;


/***
 * Entidade que representa um jogador no sistema.
 */
namespace Domain.Entities
{
    public class Player : Users
    {
        public Position Position { get; set; }

        public const int minHeight = 100;

        public const int maxHeight = 250;

        [Range(minHeight, maxHeight, ErrorMessage = "Um jogador deve ter entre 100 e 250 centimetors")]
        public int Height { get; set; }

        public Teams? Team { get; set; }

        public Guid? idTeam { get; set; } //FK

        public bool IsAdmin { get; set; } //Validar se é mesmo necessário

        [Range(0, int.MaxValue, ErrorMessage = "O número de convites tem de ser pelo menos 0")]
        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        // EF
        protected Player() { }

        public Player(string name, DateOnly dateOfBirth, string address, string email, string password, int phoneNumber, Position position, int height)
            : base(name, dateOfBirth, address, email, password, phoneNumber)
        {
            Position = position;
            Height = height;
            IsAdmin = false;
        }

        //Falta meter o CRUD básico do MembershipRequests
        /***
         * Metodo que permite encontrar um pedido de adesão a uma equipa pelo seu id
         * 
         * idMembershipRequest: O id do pedido de adesão a encontrar
         * 
         * Retorna o pedido de adesão com o id especificado ou null se não for encontrado
         */
        private MembershipRequests FindMembershipRequest(Guid idMembershipRequest)
        {
            return null;
        }

        /***
         * Metodo que permite adicionar um pedido de adesão a uma equipa
         * 
         * membershipRequests: O pedido de adesão a adicionar
         * 
         * Retororna o pedido de adesão adicionado ou null se não for possível adicionar
         */
        public MembershipRequests addMemberShipRequest(MembershipRequests membershipRequests) {
            return null;
        }

        /***
         *  Metodo que permite remover um pedido de adesão a uma equipa
         *  
         *  membershipRequests: O pedido de adesão a remover
         *  
         *  Retorna o pedido de adesão removido ou null se não for possível remover
         */
        public MembershipRequests removeMembershipRequest(MembershipRequests membershipRequests) {
            return null;
        }

        /***
         * Metodo que permite obter um pedido de adesão a uma equipa pelo seu id
         * 
         * id: O id do pedido de adesão a obter
         * 
         * Retorna o pedido de adesão com o id especificado ou null se não for encontrado
         */
        public MembershipRequests getMembershipRequestById(Guid id) {
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + $", Position: {Position}, Height: {Height}cm, Team: {(Team != null ? Team.Name : "No Team")}, IsAdmin: {IsAdmin}";
        }
    }
}
