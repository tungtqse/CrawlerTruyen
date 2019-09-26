using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWorkFactory<out TUnitOfWork> where TUnitOfWork : IUnitOfWork
    {
        TUnitOfWork Create();
    }
}
