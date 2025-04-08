using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class DeviceRepository
    {
        public static Device AddNewDevice(User user)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                Device Device = new Device { Id = Guid.NewGuid(), OwnerId = user.id };
                context.Devices.Add(Device);

                return Device;
            }
        }
    }
}
