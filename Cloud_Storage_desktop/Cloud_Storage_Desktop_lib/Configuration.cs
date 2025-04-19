using Cloud_Storage_Desktop_lib.Interfaces;
using Lombok.NET;

namespace Cloud_Storage_Desktop_lib
{
    [ToString]
    public partial class Configuration : IConfiguration
    {
        private const string _ApiUrl = "http://localhost:5087";

        public string ApiUrl
        {
            get { return _ApiUrl; }
        }
        private string _StorageLocation = "";

        private const int _MaxStimulationsFileSync = 5;

        public int MaxStimulationsFileSync
        {
            get { return _MaxStimulationsFileSync; }
        }

        public string DeviceUUID
        {
            get
            {
                throw new NotImplementedException("not  yet implemned");
                return "Not implented";
            }
        }

        //[RegularExpression(
        //    @"^@""^[a-zA-Z]:\\(?:[a-zA-Z0-9 _-]+\\)*[a-zA-Z0-9 _-]+\.txt$""",
        //    ErrorMessage = "Path string doesn't match path syntax"
        //)]
        public string StorageLocation
        {
            get { return _StorageLocation; }
            set { _StorageLocation = value; }
        }
    }
}
