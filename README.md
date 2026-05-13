# FIAP Cloud Games - Catalog API

API REST em .NET 9 para catálogo, pedidos, avaliações, busca avançada e integração assíncrona.

## Arquitetura atual

- API em container publicada no AKS.
- Imagem construída no GitHub Actions e enviada para o ACR.
- Segredos sensíveis lidos do Azure Key Vault via Secrets Store CSI Driver.
- Identidade da aplicação configurada com Workload Identity.
- Mensageria única: Azure Service Bus.
- Persistência relacional via SQL Server.
- Busca e avaliações usando Elasticsearch e MongoDB.

## O que não existe mais

- RabbitMQ foi removido.
- Não há segredos reais em `appsettings.json`, Docker Compose ou manifests Kubernetes.
- O deploy não usa Azure Web App.

## Configuração

### Sensível, vindo do Key Vault

- `ConnectionStrings--SqlConnection`
- `ConnectionStrings--SetupConnection`
- `Jwt--Key`
- `ServiceBus--ConnectionString`
- `ApplicationInsights--ConnectionString`
- `MongoDb--ConnectionString`
- `Secrets--Password`

### Não sensível, vindo do ConfigMap

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ASPNETCORE_HTTP_PORTS`
- `Jwt__Issuer`
- `Jwt__Audience`
- `ServiceBus__QueueNameOrderPlaced`
- `ServiceBus__QueueNamePaymentProcessed`
- `ServiceBus__SubscriptionPaymentProcessed`
- `Elasticsearch__Uri`
- `MongoDb__Database`
- `MongoDb__ReviewCollection`

## Deploy no AKS

Manifests principais em `k8s/`:

- `serviceaccount.yaml`
- `secretproviderclass.yaml`
- `configmap.yaml`
- `deployment.yaml`
- `service.yaml`
- `ingress.yaml`

Fluxo do pipeline:

1. restore
2. build
3. test
4. login no Azure
5. login no ACR
6. build e push da imagem
7. `kubectl` no AKS
8. apply dos manifests
9. update da imagem do Deployment
10. rollout status e diagnóstico se falhar

## Secrets do GitHub Actions

Use exatamente estes nomes:

- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `ACR_NAME`
- `ACR_LOGIN_SERVER`
- `IMAGE_NAME`
- `AKS_CLUSTER_NAME`
- `AKS_RESOURCE_GROUP`
- `AKS_NAMESPACE`

Para o `SecretProviderClass`, o workflow também espera a variável de repositório `KEYVAULT_NAME`.

## Execução local

### Docker Compose

O `docker-compose.yaml` foi mantido apenas como apoio local, sem segredos reais. Preencha os placeholders via ambiente antes de subir.

```powershell
docker compose up --build -d
```

### App settings

Os arquivos de configuração mantêm apenas placeholders e valores não sensíveis. O runtime espera os segredos sensíveis vindos do ambiente/Kubernetes.

## Endpoints

- `GET /api/Games`
- `GET /api/Games/{id}`
- `POST /api/Games`
- `PUT /api/Games/{id}`
- `DELETE /api/Games/{id}`
- `GET /api/Games/search?q=...`
- `POST /api/Games/{id}/reviews`
- `GET /api/Games/{id}/reviews`
- `POST /api/Order`

## Observações técnicas

- A API lê configuração via `IConfiguration`.
- Não há leitura direta de segredos por `Environment.GetEnvironmentVariable` em runtime.
- MongoDB e Elasticsearch são configurados por `IConfiguration`.
- SQL, JWT, Service Bus e demais segredos vêm do Key Vault.

## Troubleshooting

- Falha de rollout no AKS: verifique `kubectl describe deployment/catalog-api` e os logs do pod.
- Falha de conexão com SQL: valide `ConnectionStrings--SqlConnection` no Key Vault.
- Falha de autenticação: valide `Jwt--Key`, `Jwt__Issuer` e `Jwt__Audience`.
- Falha de mensageria: valide `ServiceBus--ConnectionString`.
