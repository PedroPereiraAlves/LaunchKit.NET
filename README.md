
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
- ✅ **Logging com Serilog**
- ✅ **AutoMapper + DTOs**
- ✅ **Tratamento global de erros (Middleware)**
- ✅ **Respostas padronizadas (Sucesso/Erro)**
- ✅ **Swagger configurado**
- ✅ **Pronto para escalar com DDD / Clean Architecture**

---

## 🧱 Estrutura do Projeto

```
├── LaunchKit.Domain         # Entidades e interfaces (sem dependência externa)
├── LaunchKit.Application    # CQRS, Handlers, DTOs, AutoMapper
├── LaunchKit.Infrastructure # EF Core, Repositórios, UoW, DbContext
├── LaunchKit.API            # API REST com Controllers e Middlewares
```

---

## 📦 Como usar

```bash
git clone https://github.com/seunome/MyTemplate.git
cd MyTemplate

# Configure a connection string no appsettings.json
# Execute a migration ou use o script SQL (se fornecido)
dotnet run
```

Acesse: `https://localhost:7139/swagger`

---

## 👀 Exemplo de uso

Crie um CRUD com MediatR + Repository em minutos:
```csharp
public class CreateProductCommand : IRequest<Response<ProductDto>>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

Handler:
```csharp
public class CreateProductHandler : IRequestHandler<CreateProductCommand, Response<ProductDto>>
{
    public async Task<Response<ProductDto>> Handle(...) { ... }
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

🔔 **Cadastre-se para ser avisado:**  
[https://tally.so/r/xyz123](https://tally.so/r/xyz123)

---

## 🧑‍💻 Contribua

Quer sugerir melhorias ou usar esse projeto como base para o seu? Fique à vontade!

- ⭐ Star este repositório
- 📬 Fork e personalize para seu projeto
- 💡 Abra uma issue para feedbacks

---

## 📄 Licença

MIT — sinta-se livre para usar, modificar e compartilhar.
