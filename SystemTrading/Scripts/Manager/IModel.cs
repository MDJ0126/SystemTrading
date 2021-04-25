using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemTrading.Scripts.Manager
{
    public interface IModel
    {
        void Initialize();
        void Release();
    }
}
