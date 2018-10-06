using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Pipelines;

namespace Fractum.WebSocket.Core
{
    public sealed class PayloadPipeline : IPipeline<Payload>
    {
        public List<IPipelineStage<Payload>> Stages;

        public PayloadPipeline()
        {
            Stages = new List<IPipelineStage<Payload>>();
        }

        public IPipeline<Payload> AddStage(IPipelineStage<Payload> newStage)
        {
            Stages.Add(newStage);

            return this;
        }

        public void Clear()
            => Stages = new List<IPipelineStage<Payload>>();

        public async Task<LogMessage> CompleteAsync(Payload payload)
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

        public event Func<LogMessage, Task> Log;
    }
}