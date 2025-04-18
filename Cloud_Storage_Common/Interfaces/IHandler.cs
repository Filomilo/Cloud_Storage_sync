namespace Cloud_Storage_Common.Interfaces
{
    public interface IHandler
    {
        IHandler SetNext(IHandler handler);
        object Handle(object Request);
    }
}
