namespace InfiniteAdventure
{
    public interface IState
    {
        void Enter();
        void Tick();       // Update
        void FixedTick();  // FixedUpdate
        void Exit();
    }

    /// <summary>Minimal allocation-free FSM. States are created once by their owner and reused.</summary>
    public sealed class StateMachine
    {
        public IState Current { get; private set; }

        public void ChangeState(IState next)
        {
            if (next == null || next == Current) return;
            Current?.Exit();
            Current = next;
            Current.Enter();
        }

        public void Tick() => Current?.Tick();
        public void FixedTick() => Current?.FixedTick();
    }
}
