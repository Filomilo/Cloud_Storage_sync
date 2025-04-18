namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface ICredentialManager
    {
        void SaveToken(string token);
        string GetToken();
        void RemoveToken();
        string GetDeviceID();
    }
}
