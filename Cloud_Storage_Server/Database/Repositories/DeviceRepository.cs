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
                context.SaveChanges();
                return Device;
            }
        }

        public static Device GetDevice(string id)
        {
            using (DatabaseContext context = new DatabaseContext())
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
        }

        public static long GetUserIdByDeviceId(string deviceReuqested)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                Device Device = context.Devices.FirstOrDefault(x =>
                    x.Id.ToString() == deviceReuqested
                );

                return Device.OwnerId;
            }
        }
    }
}
