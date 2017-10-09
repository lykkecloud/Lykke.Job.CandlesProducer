namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IHaveState<TState>
    {
        TState GetState();
        void SetState(TState state);
        string DescribeState(TState state);
    }
}
