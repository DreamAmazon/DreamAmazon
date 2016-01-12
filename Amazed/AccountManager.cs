using System.Collections.Generic;
using System.Linq;
using DreamAmazon.Interfaces;

namespace DreamAmazon
{
    public class AccountManager : IAccountManager
    {
        private readonly List<Account> _accounts = new List<Account>();

        public int Count { get { return _accounts.Count; } }

        public IEnumerable<Account> Accounts { get { return _accounts; } }

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
            _accounts.Clear();
        }
    }
}