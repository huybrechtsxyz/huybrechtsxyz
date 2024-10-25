# [Consul](https://www.consul.io)

## What is Consul?
Consul is a service mesh solution providing a full featured control plane with service discovery, configuration, and segmentation functionality. It can provide Service Discovery, Health Checking, KeyValue Store for configuration, Secure Service Communication, and supports Multi Datacenters.

## Glossary
The technical terms used in the documentation for Consul.

- Agent - An agent is the long running daemon on every member of the Consul cluster. It is started by running consul agent. The agent is able to run in either client or server mode. Since all nodes must be running an agent, it is simpler to refer to the node as being either a client or server, but there are other instances of the agent.
- Client - A client is an agent that forwards all RPCs to a server. The client is relatively stateless.
- Server - A server is an agent with an expanded set of responsibilities including participating in the Raft quorum, maintaining cluster state, responding to RPC queries, exchanging WAN gossip with other datacenters, and forwarding queries to leaders or remote datacenters.
- Datacenter - Consul defines a datacenter to be a networking environment that is private, low latency, and high bandwidth. This excludes communication that would traverse the public internet.

## Architecture of Consul
Consul is a distributed, highly available system.
Every node that provides services to Consul runs a Consul agent.
The agents talk to one or more Consul servers. The Consul servers are where data is stored and replicated.
While Consul can function with one server, 3 to 5 is recommended to avoid failure scenarios leading to data loss. A cluster of Consul servers is recommended for each datacenter.
![Consul Architecture](../img/consul-architecture.png)
