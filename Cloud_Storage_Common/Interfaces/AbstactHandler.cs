namespace Cloud_Storage_Common.Interfaces
{
    public abstract class AbstactHandler : IHandler
    {
        protected IHandler _nextHandler;

        public IHandler SetNext(IHandler handler)
        {
            this._nextHandler = handler;

            return handler;
        }

        public abstract object Handle(object request);
    }
}
