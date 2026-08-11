# 🧱 LaunchKit.NET - CRUD Genérico com CQRS + Repository + UoW

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![Open Source](https://img.shields.io/badge/license-MIT-brightgreen)
![Production Ready](https://img.shields.io/badge/ready%20for-produção-orange)
![Serilog Logging](https://img.shields.io/badge/logging-Serilog-informational)

> **🚀 Um template prático, escalável e com boas práticas para criação de CRUDs em ASP.NET Core 8.**

Ideal para:
- Projetos MVP / Freelancers
- Startups que querem escalar depois
- Quem quer produtividade sem abrir mão de arquitetura limpa

---

## ✅ Features

- ✅ **.NET 8 + ASP.NET Core**
- ✅ **CQRS com MediatR** (Commands & Queries separados)
- ✅ **Repository Pattern + Unit of Work**
- ✅ **EF Core + SQLite em arquivo** (persistente; criado automaticamente na primeira execução)
- ✅ **Logging com Serilog**
- ✅ **AutoMapper + DTOs**
- ✅ **Tratamento global de erros (Middleware)**
- ✅ **Respostas padronizadas (Sucesso/Erro)**
- ✅ **Swagger configurado**
- ✅ **CRUD completo de exemplo (Products)**
- ✅ **Pronto para escalar com DDD / Clean Architecture**

---

## 🧱 Estrutura do Projeto

```
├── MyTemplate.Domain         # Entidades e interfaces (sem dependência externa)
├── MyTemplate.Application    # CQRS, Handlers, DTOs, AutoMapper
├── MyTemplate.Infrastructure # EF Core, Repositórios, UoW, DbContext
├── MyTemplate.API            # API REST com Controllers e Middlewares
├── MyTemplate.Shared         # Tipos compartilhados (Result<T>, helpers)
```

---

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 📦 Como usar

```bash
git clone https://github.com/PedroPereiraAlves/LaunchKit.NET.git
cd LaunchKit.NET

dotnet restore
dotnet run --project MyTemplate.API
```

Em seguida, abra o Swagger:

- HTTPS: [https://localhost:7139/swagger](https://localhost:7139/swagger)
- HTTP: [http://localhost:5167/swagger](http://localhost:5167/swagger)

Não é necessária configuração adicional de banco para iniciar o projeto.

### Banco de dados

O template utiliza **SQLite em arquivo** (não em memória). Na primeira execução, o EF Core cria automaticamente o arquivo `launchkit.db` na pasta de execução da API.

| Comportamento | Detalhe |
|---------------|---------|
| Persistência | Os dados permanecem entre reinícios da aplicação |
| Local do arquivo | Diretório de execução do projeto `MyTemplate.API` |
| Reset local | Remova o arquivo `launchkit.db` para recomeçar com um banco vazio |

Connection string padrão em `MyTemplate.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=launchkit.db"
}
```

Para migrar para SQL Server, PostgreSQL ou outro provedor, altere o provider em `InfrastructureServices` e atualize a connection string correspondente.

---

## 🔌 Endpoints de exemplo (Products)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/products` | Lista todos |
| `GET` | `/api/products/{id}` | Busca por Id |
| `POST` | `/api/products` | Cria |
| `PUT` | `/api/products/{id}` | Atualiza |
| `DELETE` | `/api/products/{id}` | Remove |

Exemplo de body (`POST` / `PUT`):

```json
{
  "name": "Notebook",
  "quantity": 10,
  "price": 3499.90
}
```

---

## 👀 Como criar um novo CRUD

1. Crie a entidade em `MyTemplate.Domain/Entities`
2. Adicione o `DbSet<>` (e opcionalmente um `IEntityTypeConfiguration<>`) na Infrastructure
3. Crie Commands/Queries + Handlers em `MyTemplate.Application/Features`
4. Crie o Profile do AutoMapper
5. Exponha o Controller na API

Command de exemplo:

```csharp
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

---

## 📸 Screenshots

<img src="screenshots/swagger-ui.png" width="700px" />

---

## 💼 LaunchKit.NET PRO (Em breve!)

Quer mais?

> A versão PRO inclui:
>
> - 🔐 Autenticação + Autorização JWT
> - 🧾 Auditoria + Histórico de Entidades
> - 🧰 CLI para geração de CRUD automático
> - 📊 Dashboard com métricas e health checks
> - 🔄 Integração com RabbitMQ (eventos)
> - 💾 Suporte a múltiplos bancos: PostgreSQL, MySQL

---

## 🧑‍💻 Contribua

Quer sugerir melhorias ou usar esse projeto como base para o seu? Fique à vontade!

- ⭐ Star este repositório
- 📬 Fork e personalize para seu projeto
- 💡 Abra uma issue para feedbacks

---

## 📄 Licença

MIT — sinta-se livre para usar, modificar e compartilhar. Veja o arquivo [LICENSE](LICENSE).
