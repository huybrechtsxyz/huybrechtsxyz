# Telemetry

https://hub.docker.com/r/grafana/otel-lgtm

https://github.com/grafana/docker-otel-lgtm/

aka consul

![telemetry-architecture](../img/grafana-otel-lgtm.png)

The grafana/otel-lgtm Docker image is an open source backend for OpenTelemetry that’s intended for development, demo, and testing environments. If you are looking for a production-ready, out-of-the box solution to monitor applications and minimize MTTR (mean time to resolution) with OpenTelemetry and Prometheus.

## Enable logging

You can enable logging for troubleshooting:

| Environment Variable | Enable Logging in |
| --- | --- |
| ENABLE_LOGS_GRAFANA | Grafana |
| ENABLE_LOGS_LOKI | Loki |
| ENABLE_LOGS_PROMETHEUS | Prometheus |
| ENABLE_LOGS_TEMPO | Tempo |
| ENABLE_LOGS_OTELCOL | OpenTelemetry Collector |
| ENABLE_LOGS_ALL | all of the above |

This has nothing to do with the application logs, which are collected by OpenTelemetry.

## Persist data across container instantiation

The various components in the repository are configured to write their data to the /data directory. If you need to persist data across containers being created and destroyed, you can mount a volume to the /data directory. Note that this image is intended for development, demo, and testing environments and persisting data to an external volume doesn't change that. However, this feature could be useful in certain cases for some users even in testing situations.

## View Grafana

Log in to http://localhost:3000 with user admin and password admin.

## Prometheus Monitoring

Prometheus is an open-source systems monitoring and alerting toolkit designed for reliability and scalability. In our system, Prometheus is used to collect metrics from various services, store them efficiently, and enable real-time alerting and visualization.

### Key Features

1. **Metrics Collection**: Aggregates metrics from:
   - Application services.
   - System-level metrics.
   - Third-party services via exporters.

2. **Alerting**: Configured with rules for real-time notifications via [Alertmanager](https://prometheus.io/docs/alerting/latest/alertmanager/).

3. **Data Visualization**: Integrated with [Grafana](https://grafana.com/) for rich, customizable dashboards.

4. **Scalability**: Supports high availability and long-term storage of metrics.

### Architecture Overview

1. **Prometheus Server**:

   - Scrapes metrics from targets (defined in `prometheus.yml`).
   - Stores data in a time-series database.

2. **Exporters**:

   - **Node Exporter**: System metrics (CPU, memory, disk).
   - **Custom Exporters**: Application-specific metrics.
   - **Third-party Exporters**: Databases, cloud services.

3. **Alertmanager**:

   - Manages alert notifications.
   - Routes alerts to email, Slack, PagerDuty, etc.

4. **Visualization**:

   - Metrics visualized in Grafana using Prometheus as a data source.
