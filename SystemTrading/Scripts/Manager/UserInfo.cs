using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemTrading.Scripts.Manager
{
    public partial class UserInfo : IModel
    {
        public string Id { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        public void Initialize()
        {
            Id = string.Empty;
            Password = "";
        }

        public void Release()
        {
            Id = string.Empty;
            Password = string.Empty;
        }
    }
}
