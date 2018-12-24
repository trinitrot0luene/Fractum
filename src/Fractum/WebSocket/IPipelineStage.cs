using System.Threading.Tasks;
using Fractum;

namespace Fractum.WebSocket
{
    public interface IPipelineStage<TData>
    {
        /// <summary>
        ///     Asynchronously enter the <see cref="IPipelineStage{TData}" /> and execute it.
        /// </summary>
        /// <param name="data">The data to operate on.</param>
        /// <param name="ctx">Contextual information about the <see cref="TData"/> being processed.</param>
        /// <returns></returns>
        Task CompleteAsync(TData data, PipelineContext ctx);
    }
}