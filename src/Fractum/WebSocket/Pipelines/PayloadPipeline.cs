using Fractum.Entities;
using Fractum.WebSocket;
using Fractum.WebSocket.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class PayloadPipeline : IPipeline<Payload>
    {
        public IPipelineStage<Payload>[] Stages;

        public IFractumCache Cache { get; }

        public IStateCache State { get; }

        public SocketWrapper Socket { get; }

        public PayloadPipeline(IFractumCache cache, IStateCache state, SocketWrapper socket)
        {
            Cache = cache;
            State = state;
            Socket = socket;
            Stages = new PipelineStage[0];
        }

        public void AddStage(IPipelineStage<Payload> newStage)
        {
            Array.Resize(ref Stages, Stages.Length + 1);
            Stages[Stages.Length - 1] = newStage; 
        }

        public void Clear()
            => Stages = new PipelineStage[0];

        public async Task<LogMessage> CompleteAsync(Payload payload)
        {
            var exceptions = new List<Exception>();
            for(int pipelinePos = 0; pipelinePos < Stages.Length; pipelinePos++)
            {
                try
                {
                    await Task.Yield();

                    await Stages[pipelinePos].CompleteAsync(payload);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            return exceptions.Count == 0 ? null : new LogMessage(nameof(PayloadPipeline), "Errors occured while completing the payload pipeline.",
                LogSeverity.Error, new AggregateException("An exception was thrown while completing one or more stages in the pipeline.", exceptions));
        }
    }
}
