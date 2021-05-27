using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.Threading
{
    public class Dispatcher
    {
        public static readonly Dispatcher UIThread = new();
        public void VerifyAccess() { }
    }
}
