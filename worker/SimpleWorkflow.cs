using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Temporalio.Workflows;

namespace DotNetTemplate;

public record SimpleInput(String val);
public record SimpleOutput(String result);

[Workflow]
public class SimpleWorkflow {

    ActivityOptions activityOptions = new () {
        StartToCloseTimeout = TimeSpan.FromSeconds(5),
        RetryPolicy = new() {
            InitialInterval = TimeSpan.FromSeconds(1),
            BackoffCoefficient = 2,
            MaximumInterval = TimeSpan.FromSeconds(30)
        }
    };

    [WorkflowRun]
    public async Task<SimpleOutput> RunAsync(SimpleInput input)
    {
        var logger = Workflow.Logger;
        logger.LogDebug($"Simple workflow started, input = {input.val}");

        var result1 = await Workflow.ExecuteActivityAsync(
            (MyActivities act) => act.Echo1(input.val),activityOptions);
    
        var result2 = await Workflow.ExecuteActivityAsync(
            (MyActivities act) => act.Echo2(input.val),activityOptions);

        var result3 = await Workflow.ExecuteActivityAsync(
            (MyActivities act) => act.Echo3(input.val),activityOptions);

        logger.LogDebug("Sleeping for 1 second...");
        await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

        var echoInput = new EchoInput(result3);
        // start the activity async
        var echo4Task = Workflow.ExecuteActivityAsync(
            (MyActivities act) => act.Echo4(echoInput), activityOptions);
        // do some other things
        // get the result from the async activity (and wait if necessary)

        // If you want to test what happens when the workflow is cancelled 
        // then uncomment out the next line to give you some time to 
        // cancel in the UX
        //
        await Workflow.DelayAsync(TimeSpan.FromSeconds(10));
        var result4 = await echo4Task;

        return new SimpleOutput(result4.result);
    }
}