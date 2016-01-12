using System.Collections.Generic;

namespace DreamAmazon.Interfaces
{
    public interface IAccountManager
    {
        void QueueAccount(string email, string password);
        void Clear();
        int Count { get; }
        IEnumerable<Account> Accounts { get; }
    }
}