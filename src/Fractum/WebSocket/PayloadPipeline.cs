using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;
using Microsoft.Extensions.DependencyInjection;

namespace Fractum.WebSocket
{
    public sealed class PayloadPipeline : IPipeline<IPayload<EventModelBase>>
    {
        public List<IPipelineStage<IPayload<EventModelBase>>> Stages;

        internal PayloadPipeline()
        {
            Stages = new List<IPipelineStage<IPayload<EventModelBase>>>();
        }

        /// <summary>
        ///     Add a stage to be executed by the pipeline.
        /// </summary>
        /// <param name="newStage"></param>
        /// <returns></returns>
        public IPipeline<IPayload<EventModelBase>> AddStage(IPipelineStage<IPayload<EventModelBase>> newStage)
        {
            Stages.Add(newStage);

            return this;
        }

        /// <summary>
        /// Get a stage added to the pipeline.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPipelineStage<IPayload<EventModelBase>> GetStage<T>()
            => Stages.FirstOrDefault(s => s.GetType() == typeof(T));

        /// <summary>
        ///     Remove all stages from the pipeline.
        /// </summary>
        public void Clear()
            => Stages = new List<IPipelineStage<IPayload<EventModelBase>>>();

        /// <summary>
        ///     Enter the pipeline and begin asynchronously processing stages.
        /// </summary>
        /// <param name="payload"><see cref="IPayload{T}" /> to be processed by the pipeline during execution.</param>
        /// <param name="context"><see cref="PipelineContext"/> providing contextual information to pipeline stages during completion.</param>
        /// <returns></returns>
        public async Task<LogMessage> CompleteAsync(IPayload<EventModelBase> payload, PipelineContext context)
        {
            var exceptions = new List<Exception>();
            for (var pipelinePos = 0; pipelinePos < Stages.Count; pipelinePos++)
                try
                {
                    await Task.Yield();

                    await Stages[pipelinePos].CompleteAsync(payload, context);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            return exceptions.Count == 0
                ? null
                : new LogMessage(nameof(PayloadPipeline), "Errors occured while completing the payload pipeline.",
                    LogSeverity.Error,
                    new AggregateException(
                        "An exception was thrown while completing one or more stages in the pipeline.", exceptions));
        }
    }
}