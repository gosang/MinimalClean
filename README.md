# 📖 Project Overview: .NET 8 Minimal API

## 🏗️ Architecture

Minimal API: Built with .NET 8’s lightweight HTTP pipeline, using Program.cs as the single entry point.

## Layered Design:

- Domain Layer: Core business logic and domain events.

- Application Layer: Event handlers, services, and abstractions.

- Infrastructure Layer: Persistence (EF Core), RabbitMQ integration (Outbox/Inbox/DLQ), notifications.

- Presentation Layer: Minimal API endpoints exposing RESTful routes.

## Event‑Driven Reliability:

- Outbox Pattern: Ensures reliable publishing of domain events to RabbitMQ.

- Inbox Pattern: Deduplicates consumed events using PayloadHash.

- Dead‑Letter Queue (DLQ): Captures failed messages for monitoring and alerting.

## Background Services:

- OutboxPublisher: Publishes pending events to RabbitMQ.

- InboxConsumer: Subscribes to domain_events exchange, deduplicates, and dispatches handlers.

- DlqConsumer: Monitors DLQ and triggers alerts.

- Cleanup workers for Outbox/Inbox TTL management.

✨ Key Features

- .NET 8 Minimal API: Fast startup, reduced boilerplate, async‑first design.

- Entity Framework Core: Database persistence with migrations and LINQ queries.

## RabbitMQ Integration:

- Async channel API (CreateChannelAsync, BasicPublishAsync, BasicConsumeAsync).

- Exchange/queue declarations with DLX support.

## Resilience & Reliability:

- Polly pipelines for retries and exponential backoff.

- Deduplication via Inbox records.

- DLQ routing for failed messages.

## Observability:

- Structured logging with ILogger.

- Metrics hooks for published/consumed/failed events.

## Notifications:

- SendGrid integration for email alerts.

- Batch DLQ alerting to avoid inbox flooding.

## Security & Config:

- RabbitMQ credentials and SendGrid API keys stored in appsettings.json / secrets.

- TLS support for RabbitMQ connections.

🚀 Getting Started

- Clone & Restore

```bash
git clone <repo-url>
dotnet restore
```

- Configure Settings

- Update appsettings.json with RabbitMQ and SendGrid credentials.

- Run Migrations

```bash
dotnet ef database update
```

Run the API

```bash
dotnet run
```

Minimal API endpoints will be available at https://localhost:<port>.

📊 Reliability Loop

```text
Domain Event → Outbox → RabbitMQ Exchange → Inbox Consumer → Deduplication → Handler
↓
Dead-Letter Queue → Alerts (SendGrid)
```

🔮 Next Improvements

- Add OpenTelemetry tracing for distributed observability.
- Extend DLQ consumer with Slack/PagerDuty integration.
- Implement retry queues with TTL for controlled reprocessing.
- Harden security with secrets manager and TLS certificates.
