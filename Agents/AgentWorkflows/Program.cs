using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2");

// Sequential Workflow : The simplest pattern: output from one agent becomes input to the next.
// Create specialized agents
AIAgent researcher = chatClient.AsAIAgent(
    name: "Researcher",
    instructions: "Research and gather key facts about the topic.");

AIAgent writer = chatClient.AsAIAgent(
    name: "Writer",
    instructions: "Write stories that are engaging and creative.");

AIAgent editor = chatClient.AsAIAgent(
    name: "Editor",
    instructions: "Make the story more engaging, fix grammar, and enhance the plot.");

AIAgent factChecker = chatClient.AsAIAgent(
    name: "FactChecker",
    instructions: "Verify claims and add [citation needed] where facts cannot be confirmed.");

// Build sequential workflow
Workflow workflow = AgentWorkflowBuilder.BuildSequential(researcher, writer, editor, factChecker);

// Run the workflow
AIAgent seqWorkflowAgent = workflow.AsAIAgent();
AgentResponse seqResponse = await seqWorkflowAgent.RunAsync("Write a short story about a haunted house.");
Console.WriteLine(seqResponse.Text);

// Concurrent Workflow: When agents can work independently on the same input.
AIAgent sentimentAnaylist = chatClient.AsAIAgent(
    name: "SentimentAnalyst",
    instructions:"Analyze the emotional tone and sentiment of the text.");

AIAgent summaryAgent = chatClient.AsAIAgent(
    name: "Summarizer",
    instructions: "Provide a concise summary of the text.");

AIAgent keywordExtractor = chatClient.AsAIAgent(
    name: "KeywordExtractor",
    instructions: "Extract the main keywords and topics from the text.");

// Build concurrent workflow - all agents process in parallel
Workflow concurrent = AgentWorkflowBuilder.BuildConcurrent(
    "TextAnalysis",
    new[] { sentimentAnaylist, summaryAgent, keywordExtractor });

var conResponse = await concurrent.AsAIAgent().RunAsync("""
    The new product launch exceeded all expectations. Sales were 
    up 200% compared to last year, and customer feedback has been 
    overwhelmingly positive. The marketing team's innovative 
    campaign drove significant social media engagement.
    """);

// Response contains aggregated results from all agents
Console.WriteLine(conResponse.Text);

// Handoff Workflow: Dynamic routing where agents decide when to pass control:

// Support agent for general questions
AIAgent generalSupport = chatClient.AsAIAgent(
    name: "GeneralSupport",
    instructions: """
        You handle general customer questions.
        If the customer has a billing issue, hand off to BillingSpecialist.
        If the customer has a technical issue, hand off to TechnicalSupport.
        """);
// Specialist for billing issues
AIAgent billingSpecialist = chatClient.AsAIAgent(
    name: "BillingSpecialist",
    instructions: """
        You are a billing expert. Handle payment, invoice, and 
        subscription questions. If the issue is resolved, you can 
        hand back to GeneralSupport for any follow-up questions.
        """);

// Specialist for technical issues
AIAgent technicalSupport = chatClient.AsAIAgent(
    name: "TechnicalSupport",
    instructions: """
        You are a technical support expert. Handle software bugs,
        configuration issues, and how-to questions.
        """);

// Build handoff workflow - the starting agent decides when to hand off
Workflow handoff = AgentWorkflowBuilder.CreateHandoffBuilderWith(generalSupport)
    .WithHandoffs(generalSupport, [billingSpecialist, technicalSupport])
    .WithHandoff(billingSpecialist, generalSupport)
    .Build();

// First query goes to GeneralSupport, which may route to specialists
var handResponse = await handoff.AsAIAgent().RunAsync("I was charged twice for my subscription last month.");
Console.WriteLine(handResponse.Text);