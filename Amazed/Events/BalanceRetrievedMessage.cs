namespace DreamAmazon.Events
{
    public class BalanceRetrievedMessage
    {
        public double Balance { get; protected set; }

        public BalanceRetrievedMessage(double balance)
        {
            Balance = balance;
        }
    }
}