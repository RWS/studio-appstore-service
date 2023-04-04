using AppStoreIntegrationServiceCore.DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IAccountEntitlementsManager
    {
        IEnumerable<AccountEntitlement> Entitlements { get; }
        void Add(AccountEntitlement entitlement);
    }
}
