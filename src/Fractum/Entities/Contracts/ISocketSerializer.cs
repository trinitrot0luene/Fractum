using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities.Contracts
{
    internal interface ISocketSerializer
    {
        string Deserialize();

        byte[] Serialize();
    }
}
