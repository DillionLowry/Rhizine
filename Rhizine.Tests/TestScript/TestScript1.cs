using Rhizine.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhizine.Tests.TestScript
{
    public class TestScript1 : ITestScript
    {
        public string Name => "Test Case 1";

        public string Description => throw new NotImplementedException();

        public List<string> Prerequisites => throw new NotImplementedException();

        public bool CanExecute()
        {
            throw new NotImplementedException();
        }

        public TestScriptResult Execute()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Simulate work
            System.Threading.Thread.Sleep(1000);
            stopwatch.Stop();

            return new TestScriptResult
            {
                Name = Name,
                Status = "Success",
                Duration = stopwatch.ElapsedMilliseconds,
                Remarks = "Completed successfully."
            };
        }
    }
}
