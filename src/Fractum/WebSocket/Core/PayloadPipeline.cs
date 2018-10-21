using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Core
{
    public sealed class PayloadPipeline : IPipeline<IPayload<EventModelBase>>
    {
        public List<IPipelineStage<IPayload<EventModelBase>>> Stages;

        public PayloadPipeline()
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
        ///     Remove all stages from the pipeline.
        /// </summary>
        public void Clear()
            => Stages = new List<IPipelineStage<IPayload<EventModelBase>>>();

        /// <summary>
        ///     Asynchronously enter the pipeline and begin processing stages.
        /// </summary>
        /// <param name="payload"><see cref="Payload"/> to be processed by the pipeline during execution.</param>
        /// <returns></returns>
        public async Task<LogMessage> CompleteAsync(IPayload<EventModelBase> payload)
        {
            var exceptions = new List<Exception>();
            for (var pipelinePos = 0; pipelinePos < Stages.Count; pipelinePos++)
                try
                {
                    await Task.Yield();

                    await Stages[pipelinePos].CompleteAsync(payload);
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

        private void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        /// <summary>
        ///     Raised when the <see cref="PayloadPipeline"/> encounters an error.
        /// </summary>
        public event Func<LogMessage, Task> Log;
    }
}