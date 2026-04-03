# FIAP Cloud Games - Catalog API

Uma API REST em .NET 9 para gerenciar jogos. Responsável por cadastro, autenticação (geração de token JWT) e autorização de jogos. Pronta para rodar em container com Docker e orquestração via Docker Compose. Inclui inicialização do banco, seed de usuário de teste e integração com RabbitMQ.

## Sumário
- Sobre
- Tecnologias
- Pré-requisitos
- Variáveis e configurações importantes
- Executando com Docker Compose (modo recomendado)
- Acessando a API (Swagger & endpoints)
- Exemplos de requests (PowerShell e curl)
- DTOs / Exemplos de payload
- Banco de dados & Seed
- RabbitMQ & Painel de gestão
- Segurança e produção
- Executando localmente (opcional)
- Testes
- Troubleshooting
- Contribuição

## Sobre
Esta API fornece endpoints para autenticação (`Login`) e para operações CRUD/administrativas de jogos e bibliotecas (`GamesController` e `OrderController`). O projeto já inclui scripts para criar o banco e um seed inicial com um usuário admin de teste.

## Tecnologias
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- MongoDB (avaliações de jogos)
- Elasticsearch
- Docker / Docker Compose
- RabbitMQ
- JWT (autenticação)
- Swagger (documentação)

## Pré-requisitos
- Docker Desktop instalado e em execução (Compose v2 recomendado).
- Pelo menos 4GB de RAM livre ao rodar a stack completa (SQL Server + RabbitMQ + API).
- Git (opcional).

Exemplos de comandos abaixo são para PowerShell (Windows).

## Variáveis e configurações importantes
As configurações padrão estão em `src/Catalog.Api/appsettings.json` e o `docker-compose.yaml` define as variáveis de ambiente para o container `api`.

Principais chaves:
- Connection Strings
	- `ConnectionStrings:SetupConnection` — usada para criar/migrar banco e criar o login `usuario_app`.
	- `ConnectionStrings:DefaultConnection` — conexão usada pela aplicação.
- JWT
	- `Jwt:Key` — chave simétrica (trocar em produção)
	- `Jwt:Issuer`, `Jwt:Audience`
- Secrets
	- `Secrets:Password`
- RabbitMQ
	- `RabbitMq:HostName`, `RabbitMq:Port`, `RabbitMq:UserName`, `RabbitMq:Password`, `RabbitMq:ExchangeName`

Valores padrão do repositório (exemplos):
- SA SQL Server: `SenhaForte123!`
- Usuário application criado: `usuario_app` / `SenhaForte123!`
- Seed admin: `teste@cloudgames.com.br` / `SenhaForte123!` (Role = Admin)
- Porta exposta da API: 5055

> Atenção: troque todas as credenciais antes de usar em produção.

## Executando com Docker Compose (recomendado)
O arquivo `docker-compose.yaml` sobe os serviços:
- `mssql` (SQL Server 2022)
- `mongo` (armazenamento de avaliações)
- `rabbitmq` (com management UI)
- `api` (construída pelo `Dockerfile`)

Na raiz do projeto (onde está `docker-compose.yaml`) execute em PowerShell:

```powershell
docker compose up --build -d
```

Verificar status dos serviços:

```powershell
docker compose ps
```

Ver logs da API:

```powershell
docker compose logs -f api
```

Parar e remover a stack:

```powershell
docker compose down
```

O `docker-compose.yaml` já injeta variáveis de ambiente (connection strings, jwt, rabbitmq). Para sobrescrever localmente use um `.env` ou ajuste o `environment` no `docker-compose.yaml`.

## Acessando a API e documentação (Swagger)
A UI do Swagger está configurada na raiz da aplicação (RoutePrefix = string.Empty). Depois de subir os serviços, abra:

http://localhost:5063/

Lá você encontra a documentação interativa com todos os endpoints.

## Endpoints principais
Base: `http://localhost:5063/api`

- POST `/api/Games`
	- Cadastra um jogo (não-admin). Body: `GameDto`.
- POST `/api/Order`
	- Cria uma ordem de compra — Body `CreateOrderDto`.
- GET `/api/Games`
	- Lista todos os jogos.
- GET `/api/Games/{id}`
	- Lista o jogos específico por Id.
- GET `/api/ListGamesByUserId/{userId}`
	- Lista os jogos de determinado usuário.
- GET `/api/Games/search?q={termo}`
	- Busca avançada no Elasticsearch com fuzzy search e ordenação por relevância.
- POST `/api/Games/{id}/reviews`
	- Adiciona uma avaliação para o jogo `{id}` em MongoDB.
- GET `/api/Games/{id}/reviews`
	- Lista avaliações do jogo `{id}` a partir do MongoDB.
- DELETE `/api/Games/{id}`
	- Exclui um jogo.
- PUT `/api/Games/{id}`
	- Edita um jogo.

## Busca avançada e reindexação (Elasticsearch)
- O endpoint `/api/Games/search` usa índice dedicado no Elasticsearch (independente das consultas do banco relacional).
- Sempre que um jogo é criado ou editado, a API atualiza automaticamente o índice.
- Para manutenção interna, existe o endpoint:
	- POST `/api/Games/internal/reindex`
	- Reindexa toda a base de jogos do SQL Server para o Elasticsearch e retorna quantidade indexada.
	- A rota é interna e aparece no Swagger para facilitar testes operacionais em ambiente local.

## DTOs / Exemplos de payload
Campos reais extraídos do código (arquivo `src/Catalog.Domain/Dto`):

- `CreateGameDto`
	```json
	{
	  "id": "id-do-jogo",
	  "name": "Teste",
	  "genre": "Ação",
	  "price": 130,
	  "promotionalPrice": 120,
	  "published": true,
	  "active": true
	}
	```

- `CreateOrderDto`
	```json
	{
	  "id": "id-da-ordem",
	  "userId": "id-do-usuario",
	  "gameId": "id-do-jogo",
	  "price": 120,
	  "status": 0,
	  "createdAt": "2026-01-09T17:59:10.3681884Z"
	}
	```

## Banco de dados e seed
- O `docker-compose` expõe SQL Server na porta 1434 (alteramos a porta para não conflitar com a 1433 usada pelo SQL da API Users).
- A classe `DatabaseUserInitializer` usa `ConnectionStrings:SetupConnection` para garantir que o banco exista, aplicar migrações e criar o login `usuario_app` com role `db_owner`.
- `SeedUsuario` (API Users) adiciona `teste@cloudgames.com.br` como Admin (se não existir).

## RabbitMQ
- Porta AMQP: 5672
- Management UI: 15672 (ex.: http://localhost:15672/)
- Credenciais padrão: guest / guest

## Segurança e produção
- Troque imediatamente: `Jwt:Key`, `Secrets:Password`, senha do `sa` e do `usuario_app`.
- Use Secret Manager/Key Vault/variáveis de ambiente para segredos.
- Ative TLS/HTTPS em produção.

## Executando localmente (sem Docker) — opcional
1. Abra a solução `Catalog.slnx` no Visual Studio / VS Code.
2. Ajuste `ASPNETCORE_ENVIRONMENT=Development` e `appsettings.Development.json` se necessário.
3. No diretório `src/Catalog.Api`:

```powershell
dotnet restore
dotnet build
dotnet run --project .\\Users.Api.csproj
```

## Troubleshooting (problemas comuns)
- SQL Server não sobe: verifique recursos (memória/disco) e logs: `docker compose logs -f mssql`.
- RabbitMQ inativo: `docker compose logs -f rabbitmq`.
- 401 / Token inválido: confira `Jwt:Key`, `Issuer` e `Audience`.
- Erro ao criar usuário DB: confira `ConnectionStrings:SetupConnection` e permissões.

## Próximos passos sugeridos
- Externalizar segredos (Key Vault).
- Adicionar CI que constrói a imagem Docker e executa testes.
- Criar `.env.example` com variáveis sensíveis para desenvolvimento local.

## Contribuição
Abra issues e PRs. Mantenha os testes verdes e atualize a documentação quando adicionar novos serviços ou mudanças de contrato.

## ☸️ Kubernetes

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) com Kubernetes habilitado
- [kubectl](https://kubernetes.io/docs/tasks/tools/) (já incluso no Docker Desktop)

### Habilitar Kubernetes no Docker Desktop

1. Abra o **Docker Desktop**
2. Vá em **Settings** (ícone de engrenagem)
3. Clique em **Kubernetes** no menu lateral
4. Marque **Enable Kubernetes**
5. Clique em **Apply & Restart**
6. Aguarde o Kubernetes iniciar (ícone verde no canto inferior esquerdo)

### Deploy da Aplicação

#### Passo 1: Construir a imagem Docker

```bash
# Na raiz do projeto
docker build -t catalog-api:latest .
```

#### Passo 2: Aplicar os manifests Kubernetes

```bash
# Aplicar todos os recursos (ConfigMap, Secret, Deployment e Service)
kubectl apply -f ./k8s/
```

**Saída esperada:**
```
configmap/catalog-api-config created
deployment.apps/catalog-api created
secret/catalog-api-secret created
service/catalog-api created
```

#### Passo 3: Verificar o status

```bash
# Ver status dos pods
kubectl get pods

# Ver status dos serviços
kubectl get services

# Ver logs da aplicação
kubectl logs -f deployment/catalog-api
```

**Saída esperada:**
```
NAME                           READY   STATUS    RESTARTS   AGE
catalog-api-75b78fc9f-xxxxx   1/1     Running   0          30s
```

#### Passo 4: Acessar a aplicação

Como o Service é do tipo `ClusterIP`, use **port-forward** para acessar localmente:

```bash
kubectl port-forward service/catalog-api 5063:5063
```

A aplicação estará disponível em:
- **API:** http://localhost:5063
- **Swagger:** http://localhost:5063/swagger

### Arquivos de Configuração Kubernetes

| Arquivo | Descrição |
|---------|-----------|
| `k8s/configmap.yaml` | Configurações não-sensíveis (hostname RabbitMQ, filas, etc.) |
| `k8s/secret.yaml` | Credenciais sensíveis (usuário/senha RabbitMQ em Base64) |
| `k8s/deployment.yaml` | Definição do pod, replicas, health checks e recursos |
| `k8s/service.yaml` | Exposição do serviço internamente no cluster |

### Comandos Úteis

```bash
# Ver detalhes do pod
kubectl describe pod -l app=catalog-api

# Ver eventos do cluster
kubectl get events --sort-by='.lastTimestamp'

# Escalar replicas
kubectl scale deployment/catalog-api --replicas=3

# Atualizar após mudanças na imagem
docker build -t catalog-api:latest .
kubectl rollout restart deployment/catalog-api

# Remover todos os recursos
kubectl delete -f ./k8s/