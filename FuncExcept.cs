using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace alphaserver_cfg
{
    internal class FuncExcept
    {
        public string ExpionMsg(string _exptIn)
        {
            if (_exptIn.Contains("(0x800706BA): Сервер RPC недоступен.")) 
            {
                return "Сервер RPC недоступен(0x800706BA).Проверьте доступность IP в сети.";
            }
            return _exptIn;
        }
    }
}
