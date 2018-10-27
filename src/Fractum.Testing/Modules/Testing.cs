using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.Testing.Modules
{
    public class Testing : ModuleBase<CommandContext>
    {
        [Command("ping")]
        public Task PingAsync()
        {
            return Context.RespondAsync("Pong!");
        }
    }
}
