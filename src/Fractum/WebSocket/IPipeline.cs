using System.Threading.Tasks;
using Fractum.Entities;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Defines a pipeline for processing data in stages.
    /// </summary>
    public interface IPipeline<TData>
    {
        /// <summary>
        ///     Remove all stages from the pipeline.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Add a stage to be executed by the pipeline.
        /// </summary>
        /// <param name="newStage"></param>
        /// <returns></returns>
        IPipeline<TData> AddStage(IPipelineStage<TData> newStage);

        /// <summary>
        ///     Asynchronously enter the pipeline and begin processing stages.
        /// </summary>
        /// <param name="data">Data to be processed by the pipeline during execution.</param>
        /// <returns></returns>
        Task<LogMessage> CompleteAsync(TData data, PipelineContext context);
    }
}