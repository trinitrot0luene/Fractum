using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public interface IPipelineStage<TData>
    {
        Task CompleteAsync(TData data);
    }
}
