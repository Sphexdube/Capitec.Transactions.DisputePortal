using Capitec.Transactions.DisputePortal.Domain.Interfaces;
using Capitec.Transactions.DisputePortal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Transactions.DisputePortal.Infrastructure.Persistence.UnitOfWork
{
    internal sealed class UnitOfWorkFactory(IDbContextFactory<DisputePortalDbContext> contextFactory)
        : IUnitOfWorkFactory<IUnitOfWork>
    {
        public IUnitOfWork Create()
        {
            DisputePortalDbContext context = contextFactory.CreateDbContext();
            context.ChangeTracker.LazyLoadingEnabled = false;

            return new UnitOfWork(context);
        }
    }
}
