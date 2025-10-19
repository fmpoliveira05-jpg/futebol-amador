namespace Application.Interfaces.Repositorys
{
    public interface IUnityOfWork
    {

        /***
         * 0 --> No changed
         * 1 --> Addition
         * 2 --> Delete
         * 3 --> Update
         */
        Task<int> SaveChangesAsync();
    }
}
