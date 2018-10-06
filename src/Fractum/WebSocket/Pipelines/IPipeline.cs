using System.Threading.Tasks;
using Fractum.Entities;

namespace Fractum.WebSocket.Pipelines
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
        ///     Add a new stage to the pipeline.
        /// </summary>
        /// <param name="newStage"></param>
        IPipeline<TData> AddStage(IPipelineStage<TData> newStage);

        /// <summary>
        ///     Asynchronously enter the pipeline and begin processing stages.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<LogMessage> CompleteAsync(TData data);
    }
}