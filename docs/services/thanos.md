Introduction to Thanos

Thanos is an open-source, highly scalable, and highly available system designed to extend the capabilities of Prometheus for long-term storage and global query capabilities. Thanos helps overcome some of the limitations of Prometheus, such as storing data for long periods, scaling out Prometheus across multiple instances, and enabling powerful querying over large sets of Prometheus data.
Key Features of Thanos

    Long-Term Storage: Thanos provides a way to store Prometheus metrics indefinitely by integrating with object storage systems like S3, GCS, MinIO, etc. This allows Prometheus to continue collecting and storing metrics over time without worrying about data retention limits.

    Global Querying: Thanos enables querying across multiple Prometheus instances (or Thanos Sidecars) to provide a single global view of all your metrics. This helps you monitor and alert on data across different environments or regions.

    High Availability: Thanos ensures high availability by replicating data across multiple nodes and providing reliable failover mechanisms.

    Optimized for Scale: Thanos is designed to handle large amounts of time-series data, making it suitable for organizations with millions of metrics.

    Easy to Use: Thanos seamlessly integrates with Prometheus, leveraging existing Prometheus configurations and tooling, making it easy to adopt for teams already using Prometheus.

Thanos Components

Thanos is made up of several components that each serve a distinct purpose in a Prometheus-based monitoring system:
1. Thanos Sidecar

The Thanos Sidecar is deployed alongside a Prometheus instance and performs several key functions:

    Syncs Prometheus data to object storage for long-term retention.
    Exposes Prometheus data via HTTP for querying by Thanos Query components.
    Handles Prometheus metrics, which can be scraped by other Prometheus instances or monitoring systems.

The Thanos Sidecar integrates directly with Prometheus and allows it to scale without worrying about retention policies.

The Thanos Sidecar is used for syncing Prometheus data to an object storage (like MinIO or S3) and providing HTTP endpoints that expose important functionalities.

    Port 10901 is used by the Thanos Sidecar for the following endpoints:
        /metrics: Exposes Prometheus metrics, which can be scraped by Prometheus or other monitoring systems.
        /healthy: A health check endpoint to check if the Thanos sidecar is healthy.
        /status: Gives status information about the Thanos sidecar.

        Example of endpoints exposed by Thanos Sidecar:

    /metrics: http://<thanos-sidecar-ip>:10901/metrics
    /healthy: http://<thanos-sidecar-ip>:10901/-/healthy
    /status: http://<thanos-sidecar-ip>:10901/status

2. Thanos Query

The Thanos Query component is used for querying across multiple Prometheus instances or Thanos Sidecars. It can aggregate data from different sources and provide a unified view of metrics across your entire infrastructure.

    Query aggregation: Combines data from multiple Prometheus instances.
    Global query endpoint: Offers a single HTTP API for querying metrics.
    Dashboard: Supports Prometheus-compatible querying interfaces like PromQL.

    hanos Query (Port 10902)

The Thanos Query component is used to query long-term storage (such as Prometheus) and aggregates data from multiple Prometheus instances or Thanos Sidecars. It also exposes endpoints for health checks, querying metrics, and other purposes.

    Port 10902 is used by the Thanos Query for the following endpoints:
        /metrics: Exposes Prometheus metrics for the Thanos Query instance.
        /query: Allows users to query Prometheus metrics stored in the object storage (e.g., in MinIO).
        /health: A health check endpoint for the Thanos Query component.
        /ready: Indicates if the Thanos Query component is ready to handle requests.

Example of endpoints exposed by Thanos Query:

    /metrics: http://<thanos-query-ip>:10902/metrics
    /query: http://<thanos-query-ip>:10902/query
    /health: http://<thanos-query-ip>:10902/-/healthy
    /ready: http://<thanos-query-ip>:10902/-/ready

3. Thanos Store Gateway

The Thanos Store Gateway is responsible for serving the historical data that has been uploaded to object storage (such as S3 or MinIO). It enables access to large amounts of time-series data without the need to maintain it in local disk storage on Prometheus nodes.
4. Thanos Compactor

The Thanos Compactor component compacts the data stored in object storage to reduce storage overhead and improve query performance. It periodically merges and optimizes the data chunks stored in long-term storage.
5. Thanos Receive

Thanos Receive allows you to ingest Prometheus data from multiple Prometheus instances and store them in object storage. It’s used for situations where direct scraping from Prometheus isn’t ideal, or you want to centralize Prometheus metrics collection.
How Thanos Fits into the Monitoring Architecture

In a typical Prometheus + Thanos setup:

    Prometheus scrapes and stores time-series data locally.
    The Thanos Sidecar syncs the data to object storage (e.g., MinIO or S3) for long-term storage.
    Thanos Query can be used to query data across multiple Prometheus instances or Thanos Sidecars.
    The Thanos Store Gateway retrieves historical data from object storage when needed for querying.

This setup allows organizations to scale their monitoring solution, improve data retention, and maintain high availability for their monitoring infrastructure.
Integrating Thanos with Traefik and Prometheus

In our production environment, we use Traefik to route requests to the appropriate Thanos components, such as the Thanos Query and Thanos Sidecar services. The following describes the integration setup:

    Thanos Sidecar and Prometheus are registered with Consul for service discovery.
    Traefik uses Consul as the source of truth for routing traffic, allowing us to easily expose Thanos Query and Thanos Sidecar components via HTTP(S) routes.
    Prometheus metrics are scraped and stored in the long-term storage system (e.g., MinIO) via the Thanos Sidecar.
    Thanos Query is used to query data from the global storage, enabling powerful analytics and alerting.

    Port 10901: This is the default port for the Thanos Sidecar component. The sidecar is used to expose Prometheus metrics, and sync data between Prometheus and the object storage (like MinIO or S3). It listens on 10901 to expose internal endpoints, such as /metrics, /health, and /status.
Port 10902: This is the default port for the Thanos Query component. The query component provides the querying interface for long-term storage of metrics across various Prometheus instances or object storage systems. It listens on 10902 to expose endpoints like /query, /metrics, /health, and /ready.
