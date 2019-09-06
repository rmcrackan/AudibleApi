using System;
using System.Collections.Generic;
using System.Linq;

namespace Dinah.Core.StepRunner
{
    public class StepSequence : BaseStep
    {
        // do NOT use dictionary. order is crucial
        private List<BaseStep> steps { get; } = new List<BaseStep>();

        public void Add(BaseStep step) => steps.Add(step);
        public Func<bool> this[string name] { set => steps.Add(new BasicStep { Name = name, Fn = value }); }

        protected override bool RunRaw()
        {
            foreach (var step in steps)
            {
                var (IsSuccess, Elapsed) = step.Run();
                if (!IsSuccess)
                    return false;
            }

            return true;
        }
    }
}
