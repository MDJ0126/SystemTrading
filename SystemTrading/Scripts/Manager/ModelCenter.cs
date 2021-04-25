using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemTrading.Scripts.Manager;

public class ModelCenter
{
    public static MarketIndex MarketIndex { get; private set; } = new MarketIndex();
    public static UserInfo UserInfo { get; private set; } = new UserInfo();

    public static void Initialize()
    {
        MarketIndex.Initialize();
        UserInfo.Initialize();
    }

    public static void Release()
    {
        MarketIndex.Release();
        UserInfo.Release();
        //GC.Collect();
    }
}