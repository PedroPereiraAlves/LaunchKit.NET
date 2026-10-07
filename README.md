# LaunchKit.NET - CRUD Genérico com CQRS + Repository + UoW

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![Open Source](https://img.shields.io/badge/license-MIT-brightgreen)
![Production Ready](https://img.shields.io/badge/ready%20for-produção-orange)
![Serilog Logging](https://img.shields.io/badge/logging-Serilog-informational)

> Template prático e escalável para CRUDs em ASP.NET Core 8, com autenticação JWT, auditoria, CLI de scaffold, health checks e dashboard.

Ideal para:
- Projetos MVP / Freelancers
- Startups que querem escalar depois
- Quem quer produtividade sem abrir mão de arquitetura limpa

---

## Features

- .NET 8 + ASP.NET Core
- CQRS com MediatR (Commands & Queries)
- Repository Pattern + Unit of Work
- EF Core + SQLite em arquivo (persistente; criado na primeira execução)
- Autenticação e autorização JWT (roles Admin / User)
- Auditoria e histórico de entidades
- CLI para geração automática de CRUD
- Health checks + dashboard de métricas
- Logging com Serilog
- AutoMapper + DTOs
- Validação de entrada com FluentValidation (pipeline do MediatR)
- Tratamento global de erros no formato da API, com `traceId`
- Respostas padronizadas
- Swagger com suporte a Bearer token
- Limite de taxa em `/api/auth`, CORS explícito e cabeçalhos de segurança

---

## Estrutura do Projeto

```
├── MyTemplate.Domain         # Entidades e interfaces
├── MyTemplate.Application    # CQRS, Handlers, DTOs, AutoMapper
├── MyTemplate.Infrastructure # EF Core, Repositórios, UoW, JWT, Auditoria
├── MyTemplate.API            # API REST, Dashboard, Middlewares
├── MyTemplate.Shared         # Roles, Result<T>
├── MyTemplate.Cli            # Gerador de CRUD
```

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Como usar

```bash
git clone https://github.com/PedroPereiraAlves/LaunchKit.NET.git
cd LaunchKit.NET

dotnet restore
dotnet run --project MyTemplate.API
```

Em seguida:

- Swagger: [https://localhost:7139/swagger](https://localhost:7139/swagger) ou [http://localhost:5167/swagger](http://localhost:5167/swagger)
- Dashboard: [http://localhost:5167/dashboard](http://localhost:5167/dashboard)
- Health: [http://localhost:5167/health](http://localhost:5167/health)

### Credenciais padrão (seed)

| Campo | Valor |
|-------|-------|
| Email | `admin@launchkit.local` |
| Senha | `Admin@123` |
| Role  | `Admin` |

Altere a senha e a chave JWT antes de qualquer uso em produção.

### Banco de dados

O template utiliza **SQLite em arquivo** (não em memória). Na primeira execução, o EF Core cria o arquivo `launchkit.db` na pasta de execução da API.

| Comportamento | Detalhe |
|---------------|---------|
| Persistência | Dados permanecem entre reinícios |
| Local do arquivo | Diretório de execução de `MyTemplate.API` |
| Reset local | Remova `launchkit.db` para recriar o schema (necessário após mudanças de modelo com `EnsureCreated`) |

Connection string em `MyTemplate.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=launchkit.db"
}
```

### JWT

Issuer, audience e expiração ficam em `appsettings.json`. A chave de desenvolvimento fica somente em `appsettings.Development.json`:

```json
"Jwt": {
  "Issuer": "LaunchKit.NET",
  "Audience": "LaunchKit.NET",
  "Key": "LaunchKit.NET-Dev-Secret-Key-Change-In-Production-32+",
  "ExpirationMinutes": 60
}
```

Fora de Development a API não inicia se `Jwt:Key` estiver ausente, tiver menos de 32 bytes ou for a chave de exemplo. Em produção defina a chave por variável de ambiente (ou User Secrets em Development):

```bash
export Jwt__Key="troque-por-uma-chave-com-32-bytes-ou-mais"
```

---

## Autenticação

| Método | Rota | Acesso |
|--------|------|--------|
| `POST` | `/api/auth/register` | Público (cria role `User`) |
| `POST` | `/api/auth/login` | Público |

Exemplo de login:

```json
{
  "email": "admin@launchkit.local",
  "password": "Admin@123"
}
```

Use o token retornado no header: `Authorization: Bearer {token}`.

Pedidos sem token, com token inválido ou sem a role exigida respondem no mesmo envelope (`401` / `403`), com `message` e `traceId`.

`POST /api/auth/login` e `POST /api/auth/register` aceitam no máximo 20 requisições por minuto por IP (`AuthRateLimit` em `appsettings.json`). O excesso retorna `429`.

### Validação

Commands passam por FluentValidation antes do handler. Falhas de validação e de model binding usam o envelope da API:

```json
{
  "success": false,
  "message": "Dados inválidos.",
  "errors": {
    "name": ["O nome é obrigatório."]
  },
  "traceId": "0HN..."
}
```

Regras do exemplo Product: nome obrigatório (máximo 200), quantidade e preço maiores ou iguais a zero, preço com no máximo 2 casas decimais. Registro exige email válido e senha com pelo menos 8 caracteres, uma letra e um número. Email já cadastrado retorna `409`.

### CORS

Não há política `AllowAnyOrigin`. Informe origens explícitas quando houver um front em outro host:

```json
"Cors": {
  "AllowedOrigins": [ "https://app.seudominio.com" ]
}
```

Com a lista vazia, Development aceita apenas `localhost` e `127.0.0.1`. Nos demais ambientes nenhuma origem cruzada é liberada. O dashboard é servido pela própria API e não depende de CORS.

Fora de Development a API envia HSTS. Em todos os ambientes ela desliga o cabeçalho `Server` e envia `nosniff`, `Referrer-Policy`, `X-Frame-Options` e CSP (`script-src 'self'`). O Swagger, só em Development, fica de fora da CSP porque a UI oficial usa script inline.

### Autorização em Products

| Ação | Permissão |
|------|-----------|
| GET | Qualquer usuário autenticado |
| POST / PUT / DELETE | Role `Admin` |

---

## Endpoints Products

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/products` | Lista todos |
| `GET` | `/api/products/{id}` | Busca por Id |
| `POST` | `/api/products` | Cria |
| `PUT` | `/api/products/{id}` | Atualiza |
| `DELETE` | `/api/products/{id}` | Remove |

```json
{
  "name": "Notebook",
  "quantity": 10,
  "price": 3499.90
}
```

---

## Auditoria

Toda alteração em entidades derivadas de `BaseEntity` gera registro em `AuditLogs` (Created / Updated / Deleted), com usuário, timestamp e snapshot JSON (senha mascarada).

| Método | Rota | Acesso |
|--------|------|--------|
| `GET` | `/api/audit?take=100` | Admin (`take` vazio ou menor que 1 usa 100; o máximo é 200) |
| `GET` | `/api/audit/{entityName}/{entityId}` | Admin |

---

## Health checks e dashboard

| Recurso | Rota | Acesso |
|---------|------|--------|
| Health | `/health` | Público (JSON: `status`, `checks`, duração; sem detalhes de exceção) |
| Métricas | `/api/metrics` | Admin |
| Dashboard | `/dashboard` | UI (métricas exigem login admin) |

O dashboard exibe status de saúde, contagens (products, users, audit logs) e tempo no ar. O login admin preenche o token no navegador local; o botão Sair apaga esse token.

---

## CLI de geração de CRUD

```bash
dotnet run --project MyTemplate.Cli -- generate Order CustomerName:string Total:decimal
```

Gera entidade, configuration EF, DTO, commands/queries/handlers, validadores FluentValidation para propriedades `string`, AutoMapper profile e controller com `[Authorize]`. Também tenta adicionar o `DbSet<>` em `AppDbContext`.

Tipos suportados: `string`, `int`, `long`, `decimal`, `bool`, `Guid`, `DateTime`, `double`, `float`.

Arquivos existentes não são sobrescritos. Após gerar, remova `launchkit.db` (ou use migrations) para aplicar o novo schema.

---

## Como criar um novo CRUD (manual)

1. Entidade em `MyTemplate.Domain/Entities`
2. `DbSet<>` + `IEntityTypeConfiguration<>` na Infrastructure
3. Commands/Queries + Handlers em `MyTemplate.Application/Features`
4. Profile AutoMapper
5. Controller na API

Ou use a CLI acima.

---

## Screenshots

<img src="screenshots/swagger-ui.png" width="700px" />

---

## Roadmap (futuro)

- Integração com RabbitMQ (eventos)
- Suporte nativo a PostgreSQL / MySQL
- CLI empacotada como `dotnet tool`

---

## Contribua

- Star este repositório
- Fork e personalize para o seu projeto
- Abra uma issue para feedbacks

---

## Licença

MIT — veja o arquivo [LICENSE](LICENSE).
