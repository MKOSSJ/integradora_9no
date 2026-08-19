using System;
using System.Collections.Generic;   

namespace Plandi.Library.Models
{
    public class UserDeviceToken : BaseEntity
    {
        public string DeviceToken { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty;

        public long UserId { get; set; }
        public Usuario User { get; set; } = null!;
    }
}