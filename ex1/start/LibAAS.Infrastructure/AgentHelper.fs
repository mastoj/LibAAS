[<AutoOpen>]
module AgentHelper

type Agent<'T> = MailboxProcessor<'T>
let post (agent:Agent<'T>) message = agent.Post message
let postAsyncReply (agent:Agent<'T>) messageConstr = agent.PostAndAsyncReply(messageConstr)
