using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class DeviceRepository
    {
        public static Device AddNewDevice(AbstractDataBaseContext context, User user)
        {
            Device Device = new Device { Id = Guid.NewGuid(), OwnerId = user.id };
            context.Devices.Add(Device);
            context.SaveChangesAsync().Wait();
            return Device;
        }

        public static Device GetDevice(AbstractDataBaseContext context, string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return context.Devices.FirstOrDefault(x => x.Id == guidId);
            }
            else
            {
                return null;
            }
        }

        public static long GetUserIdByDeviceId(
            AbstractDataBaseContext context,
            string deviceReuqested
        )
        {
            Device Device = context.Devices.FirstOrDefault(x => x.Id.ToString() == deviceReuqested);

            return Device.OwnerId;
        }
    }
}
