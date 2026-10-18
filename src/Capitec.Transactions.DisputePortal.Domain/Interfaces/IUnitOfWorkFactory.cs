namespace Capitec.Transactions.DisputePortal.Domain.Interfaces
{
    public interface IUnitOfWorkFactory<out T>
        where T : IUnitOfWork
    {
        public T Create();
    }
}
