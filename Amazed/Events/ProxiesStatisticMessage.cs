namespace DreamAmazon.Events
{
    public class ProxiesStatisticMessage
    {
        public ProxiesStatistic Statistic { get; protected set; }

        public ProxiesStatisticMessage(ProxiesStatistic statistic)
        {
            Statistic = statistic;
        }
    }
}