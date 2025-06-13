namespace Cloud_Storage_Common.Requests
{
    public class SetVersionRequest
    {
        public Guid FileId { get; set; }
        public ulong Version { get; set; }
    }
}
