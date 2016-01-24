using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class AccountManager : IAccountManager
    {
        private readonly ConcurrentBag<Account> _accounts = new ConcurrentBag<Account>();

        public int Count => _accounts.Count;

        public IEnumerable<Account> Accounts => _accounts;

        public void QueueAccount(string email, string password)
        {
            if (_accounts.All(account => account.Email != email))
            {
                Account acc = new Account(email, password);
                _accounts.Add(acc);
            }
        }

        public void Clear()
        {
            while (!_accounts.IsEmpty)
            {
                Account account;
                _accounts.TryTake(out account);
            }
        }
    }
}