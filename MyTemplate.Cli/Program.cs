using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

if (!string.Equals(args[0], "generate", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Comando desconhecido: {args[0]}");
    PrintHelp();
    return 1;
}

if (args.Length < 2)
{
    Console.Error.WriteLine("Informe o nome da entidade. Ex.: generate Order CustomerName:string Total:decimal");
    return 1;
}

var entityName = args[1].Trim();
if (!Regex.IsMatch(entityName, "^[A-Z][A-Za-z0-9]*$"))
{
    Console.Error.WriteLine("O nome da entidade deve ser PascalCase (ex.: Order, ProductItem).");
    return 1;
}

List<(string Name, string Type)> properties;
try
{
    properties = ParseProperties(args.Skip(2));
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

if (properties.Count == 0)
{
    Console.Error.WriteLine("Informe ao menos uma propriedade no formato Nome:tipo (ex.: Name:string Price:decimal).");
    return 1;
}

var root = FindSolutionRoot(Directory.GetCurrentDirectory());
if (root is null)
{
    Console.Error.WriteLine("Não foi possível localizar a solution (MyTemplate.sln) a partir do diretório atual.");
    return 1;
}

var generator = new CrudGenerator(root, entityName, properties);
return generator.Run();

static List<(string Name, string Type)> ParseProperties(IEnumerable<string> args)
{
    var list = new List<(string Name, string Type)>();
    foreach (var arg in args)
    {
        var parts = arg.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new InvalidOperationException($"Propriedade inválida: '{arg}'. Use Nome:tipo.");

        if (!Regex.IsMatch(parts[0], "^[A-Z][A-Za-z0-9]*$"))
            throw new InvalidOperationException($"Propriedade '{parts[0]}' deve ser PascalCase.");

        list.Add((parts[0], NormalizeType(parts[1])));
    }

    return list;
}

static string NormalizeType(string type) => type.ToLowerInvariant() switch
{
    "string" => "string",
    "int" or "int32" => "int",
    "long" or "int64" => "long",
    "decimal" => "decimal",
    "bool" or "boolean" => "bool",
    "guid" => "Guid",
    "datetime" => "DateTime",
    "double" => "double",
    "float" => "float",
    _ => throw new InvalidOperationException($"Tipo não suportado: {type}")
};

static string? FindSolutionRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "MyTemplate.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

static void PrintHelp()
{
    Console.WriteLine("""
        LaunchKit.NET CLI

        Uso:
          dotnet run --project MyTemplate.Cli -- generate EntityName Prop:type Prop:type

        Exemplo:
          dotnet run --project MyTemplate.Cli -- generate Order CustomerName:string Total:decimal

        Tipos suportados: string, int, long, decimal, bool, Guid, DateTime, double, float
        """);
}

internal sealed class CrudGenerator
{
    private readonly string _root;
    private readonly string _entity;
    private readonly string _plural;
    private readonly IReadOnlyList<(string Name, string Type)> _properties;
    private readonly List<string> _created = new();
    private readonly List<string> _notes = new();

    public CrudGenerator(string root, string entity, IReadOnlyList<(string Name, string Type)> properties)
    {
        _root = root;
        _entity = entity;
        _plural = entity.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? entity + "es" : entity + "s";
        _properties = properties;
    }

    public int Run()
    {
        try
        {
            WriteEntity();
            WriteConfiguration();
            WriteDto();
            WriteCommandsQueries();
            WriteHandlers();
            WriteProfile();
            WriteController();
            PatchAppDbContext();

            Console.WriteLine($"CRUD gerado para '{_entity}'.");
            Console.WriteLine();
            Console.WriteLine("Arquivos criados:");
            foreach (var file in _created)
                Console.WriteLine($"  + {file}");

            if (_notes.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Próximos passos:");
                foreach (var note in _notes)
                    Console.WriteLine($"  - {note}");
            }

            Console.WriteLine();
            Console.WriteLine("Lembrete: delete launchkit.db (ou aplique migrations) para recriar o schema SQLite.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private void WriteEntity()
    {
        var props = string.Join(Environment.NewLine, _properties.Select(p =>
            $"    public {MapCSharpType(p.Type)} {p.Name} {{ get; set; }}{DefaultFor(p.Type)}"));

        var content = $$"""
            namespace MyTemplate.Domain.Entities;

            public class {{_entity}} : BaseEntity
            {
            {{props}}
            }
            """;

        WriteFile(Path.Combine(_root, "MyTemplate.Domain", "Entities", $"{_entity}.cs"), content);
    }

    private void WriteConfiguration()
    {
        var content = $$"""
            using Microsoft.EntityFrameworkCore;
            using Microsoft.EntityFrameworkCore.Metadata.Builders;
            using MyTemplate.Domain.Entities;

            namespace MyTemplate.Infrastructure.Configurations;

            public class {{_entity}}Configuration : IEntityTypeConfiguration<{{_entity}}>
            {
                public void Configure(EntityTypeBuilder<{{_entity}}> builder)
                {
                    builder.ToTable("{{_plural}}");
                    builder.HasKey(x => x.Id);
                }
            }
            """;

        WriteFile(Path.Combine(_root, "MyTemplate.Infrastructure", "Configurations", $"{_entity}Configuration.cs"), content);
    }

    private void WriteDto()
    {
        var props = string.Join(Environment.NewLine, _properties.Select(p =>
            $"    public {MapCSharpType(p.Type)} {p.Name} {{ get; set; }}{DefaultFor(p.Type)}"));

        var content = $$"""
            namespace MyTemplate.Application.DTOs;

            public class {{_entity}}Dto
            {
                public Guid Id { get; set; }
            {{props}}
                public DateTime CreatedAt { get; set; }
            }
            """;

        WriteFile(Path.Combine(_root, "MyTemplate.Application", "DTOs", $"{_entity}Dto.cs"), content);
    }

    private void WriteCommandsQueries()
    {
        var featureRoot = Path.Combine(_root, "MyTemplate.Application", "Features", _plural);
        var commandProps = string.Join(Environment.NewLine, _properties.Select(p =>
            $"    public {MapCSharpType(p.Type)} {p.Name} {{ get; set; }}{DefaultFor(p.Type)}"));

        WriteFile(Path.Combine(featureRoot, "Commands", $"Create{_entity}Command.cs"), $$"""
            using MediatR;
            using MyTemplate.Application.DTOs;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public class Create{{_entity}}Command : IRequest<{{_entity}}Dto>
            {
            {{commandProps}}
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Commands", $"Update{_entity}Command.cs"), $$"""
            using MediatR;
            using MyTemplate.Application.DTOs;
            using System.Text.Json.Serialization;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public class Update{{_entity}}Command : IRequest<{{_entity}}Dto?>
            {
                [JsonIgnore]
                public Guid Id { get; set; }

            {{commandProps}}
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Commands", $"Delete{_entity}Command.cs"), $$"""
            using MediatR;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public record Delete{{_entity}}Command(Guid Id) : IRequest<bool>;
            """);

        WriteFile(Path.Combine(featureRoot, "Queries", $"GetAll{_plural}Query.cs"), $$"""
            using MediatR;
            using MyTemplate.Application.DTOs;

            namespace MyTemplate.Application.Features.{{_plural}}.Queries;

            public class GetAll{{_plural}}Query : IRequest<IEnumerable<{{_entity}}Dto>> { }
            """);

        WriteFile(Path.Combine(featureRoot, "Queries", $"Get{_entity}ByIdQuery.cs"), $$"""
            using MediatR;
            using MyTemplate.Application.DTOs;

            namespace MyTemplate.Application.Features.{{_plural}}.Queries;

            public record Get{{_entity}}ByIdQuery(Guid Id) : IRequest<{{_entity}}Dto?>;
            """);
    }

    private void WriteHandlers()
    {
        var featureRoot = Path.Combine(_root, "MyTemplate.Application", "Features", _plural);
        var updateAssignments = string.Join(Environment.NewLine, _properties.Select(p =>
            $"        entity.{p.Name} = request.{p.Name};"));

        WriteFile(Path.Combine(featureRoot, "Commands", $"Create{_entity}Handler.cs"), $$"""
            using AutoMapper;
            using MediatR;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Domain.Entities;
            using MyTemplate.Domain.Interfaces;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public class Create{{_entity}}Handler : IRequestHandler<Create{{_entity}}Command, {{_entity}}Dto>
            {
                private readonly IUnitOfWork _unitOfWork;
                private readonly IMapper _mapper;

                public Create{{_entity}}Handler(IUnitOfWork unitOfWork, IMapper mapper)
                {
                    _unitOfWork = unitOfWork;
                    _mapper = mapper;
                }

                public async Task<{{_entity}}Dto> Handle(Create{{_entity}}Command request, CancellationToken cancellationToken)
                {
                    var entity = _mapper.Map<{{_entity}}>(request);
                    await _unitOfWork.Repository<{{_entity}}>().AddAsync(entity);
                    await _unitOfWork.CommitAsync();
                    return _mapper.Map<{{_entity}}Dto>(entity);
                }
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Commands", $"Update{_entity}Handler.cs"), $$"""
            using AutoMapper;
            using MediatR;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Domain.Entities;
            using MyTemplate.Domain.Interfaces;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public class Update{{_entity}}Handler : IRequestHandler<Update{{_entity}}Command, {{_entity}}Dto?>
            {
                private readonly IUnitOfWork _unitOfWork;
                private readonly IMapper _mapper;

                public Update{{_entity}}Handler(IUnitOfWork unitOfWork, IMapper mapper)
                {
                    _unitOfWork = unitOfWork;
                    _mapper = mapper;
                }

                public async Task<{{_entity}}Dto?> Handle(Update{{_entity}}Command request, CancellationToken cancellationToken)
                {
                    var repository = _unitOfWork.Repository<{{_entity}}>();
                    var entity = await repository.GetByIdAsync(request.Id);
                    if (entity is null)
                        return null;

            {{updateAssignments}}

                    repository.Update(entity);
                    await _unitOfWork.CommitAsync();
                    return _mapper.Map<{{_entity}}Dto>(entity);
                }
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Commands", $"Delete{_entity}Handler.cs"), $$"""
            using MediatR;
            using MyTemplate.Domain.Entities;
            using MyTemplate.Domain.Interfaces;

            namespace MyTemplate.Application.Features.{{_plural}}.Commands;

            public class Delete{{_entity}}Handler : IRequestHandler<Delete{{_entity}}Command, bool>
            {
                private readonly IUnitOfWork _unitOfWork;

                public Delete{{_entity}}Handler(IUnitOfWork unitOfWork)
                {
                    _unitOfWork = unitOfWork;
                }

                public async Task<bool> Handle(Delete{{_entity}}Command request, CancellationToken cancellationToken)
                {
                    var repository = _unitOfWork.Repository<{{_entity}}>();
                    var entity = await repository.GetByIdAsync(request.Id);
                    if (entity is null)
                        return false;

                    repository.Remove(entity);
                    await _unitOfWork.CommitAsync();
                    return true;
                }
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Queries", $"GetAll{_plural}Handler.cs"), $$"""
            using AutoMapper;
            using MediatR;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Domain.Entities;
            using MyTemplate.Domain.Interfaces;

            namespace MyTemplate.Application.Features.{{_plural}}.Queries;

            public class GetAll{{_plural}}Handler : IRequestHandler<GetAll{{_plural}}Query, IEnumerable<{{_entity}}Dto>>
            {
                private readonly IUnitOfWork _unitOfWork;
                private readonly IMapper _mapper;

                public GetAll{{_plural}}Handler(IUnitOfWork unitOfWork, IMapper mapper)
                {
                    _unitOfWork = unitOfWork;
                    _mapper = mapper;
                }

                public async Task<IEnumerable<{{_entity}}Dto>> Handle(GetAll{{_plural}}Query request, CancellationToken cancellationToken)
                {
                    var entities = await _unitOfWork.Repository<{{_entity}}>().GetAllAsync();
                    return _mapper.Map<IEnumerable<{{_entity}}Dto>>(entities);
                }
            }
            """);

        WriteFile(Path.Combine(featureRoot, "Queries", $"Get{_entity}ByIdHandler.cs"), $$"""
            using AutoMapper;
            using MediatR;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Domain.Entities;
            using MyTemplate.Domain.Interfaces;

            namespace MyTemplate.Application.Features.{{_plural}}.Queries;

            public class Get{{_entity}}ByIdHandler : IRequestHandler<Get{{_entity}}ByIdQuery, {{_entity}}Dto?>
            {
                private readonly IUnitOfWork _unitOfWork;
                private readonly IMapper _mapper;

                public Get{{_entity}}ByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
                {
                    _unitOfWork = unitOfWork;
                    _mapper = mapper;
                }

                public async Task<{{_entity}}Dto?> Handle(Get{{_entity}}ByIdQuery request, CancellationToken cancellationToken)
                {
                    var entity = await _unitOfWork.Repository<{{_entity}}>().GetByIdAsync(request.Id);
                    return entity is null ? null : _mapper.Map<{{_entity}}Dto>(entity);
                }
            }
            """);
    }

    private void WriteProfile()
    {
        var content = $$"""
            using AutoMapper;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Application.Features.{{_plural}}.Commands;
            using MyTemplate.Domain.Entities;

            namespace MyTemplate.Application.Features.{{_plural}}.Mapping;

            public class {{_entity}}Profile : Profile
            {
                public {{_entity}}Profile()
                {
                    CreateMap<{{_entity}}, {{_entity}}Dto>().ReverseMap().MaxDepth(64);
                    CreateMap<Create{{_entity}}Command, {{_entity}}>().MaxDepth(64);
                    CreateMap<Update{{_entity}}Command, {{_entity}}>()
                        .ForMember(dest => dest.Id, opt => opt.Ignore())
                        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                        .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                        .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                        .MaxDepth(64);
                }
            }
            """;

        WriteFile(Path.Combine(_root, "MyTemplate.Application", "Features", _plural, "Mapping", $"{_entity}Profile.cs"), content);
    }

    private void WriteController()
    {
        var content = $$"""
            using MediatR;
            using Microsoft.AspNetCore.Authorization;
            using Microsoft.AspNetCore.Mvc;
            using MyTemplate.API.Responses;
            using MyTemplate.Application.DTOs;
            using MyTemplate.Application.Features.{{_plural}}.Commands;
            using MyTemplate.Application.Features.{{_plural}}.Queries;
            using MyTemplate.Shared.Auth;

            namespace MyTemplate.API.Controllers;

            [ApiController]
            [Authorize]
            [Route("api/[controller]")]
            public class {{_plural}}Controller : ControllerBase
            {
                private readonly IMediator _mediator;

                public {{_plural}}Controller(IMediator mediator)
                {
                    _mediator = mediator;
                }

                [HttpGet]
                public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
                {
                    var result = await _mediator.Send(new GetAll{{_plural}}Query(), cancellationToken);
                    return Ok(new ApiResponse<IEnumerable<{{_entity}}Dto>>(true, "{{_plural}} encontrados", result));
                }

                [HttpGet("{id:guid}")]
                public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
                {
                    var result = await _mediator.Send(new Get{{_entity}}ByIdQuery(id), cancellationToken);
                    if (result is null)
                        return NotFound(new ApiResponse<{{_entity}}Dto>(false, "{{_entity}} não encontrado"));

                    return Ok(new ApiResponse<{{_entity}}Dto>(true, "{{_entity}} encontrado", result));
                }

                [HttpPost]
                [Authorize(Roles = Roles.Admin)]
                public async Task<IActionResult> Create([FromBody] Create{{_entity}}Command command, CancellationToken cancellationToken)
                {
                    var result = await _mediator.Send(command, cancellationToken);
                    return CreatedAtAction(nameof(GetById), new { id = result.Id },
                        new ApiResponse<{{_entity}}Dto>(true, "{{_entity}} criado com sucesso", result));
                }

                [HttpPut("{id:guid}")]
                [Authorize(Roles = Roles.Admin)]
                public async Task<IActionResult> Update(Guid id, [FromBody] Update{{_entity}}Command command, CancellationToken cancellationToken)
                {
                    command.Id = id;
                    var result = await _mediator.Send(command, cancellationToken);
                    if (result is null)
                        return NotFound(new ApiResponse<{{_entity}}Dto>(false, "{{_entity}} não encontrado"));

                    return Ok(new ApiResponse<{{_entity}}Dto>(true, "{{_entity}} atualizado com sucesso", result));
                }

                [HttpDelete("{id:guid}")]
                [Authorize(Roles = Roles.Admin)]
                public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                {
                    var deleted = await _mediator.Send(new Delete{{_entity}}Command(id), cancellationToken);
                    if (!deleted)
                        return NotFound(new ApiResponse<object>(false, "{{_entity}} não encontrado"));

                    return Ok(new ApiResponse<object>(true, "{{_entity}} removido com sucesso"));
                }
            }
            """;

        WriteFile(Path.Combine(_root, "MyTemplate.API", "Controllers", $"{_plural}Controller.cs"), content);
    }

    private void PatchAppDbContext()
    {
        var path = Path.Combine(_root, "MyTemplate.Infrastructure", "Context", "AppDbContext.cs");
        var text = File.ReadAllText(path);
        var dbSetLine = $"    public DbSet<{_entity}> {_plural} => Set<{_entity}>();";

        if (text.Contains($"DbSet<{_entity}>", StringComparison.Ordinal))
        {
            _notes.Add($"DbSet<{_entity}> já existe em AppDbContext.");
            return;
        }

        var marker = "public DbSet<Product> Products => Set<Product>();";
        if (!text.Contains(marker, StringComparison.Ordinal))
        {
            _notes.Add($"Adicione manualmente em AppDbContext: {dbSetLine.Trim()}");
            return;
        }

        text = text.Replace(marker, marker + Environment.NewLine + dbSetLine, StringComparison.Ordinal);
        File.WriteAllText(path, text, Encoding.UTF8);
        _notes.Add($"DbSet<{_entity}> adicionado em AppDbContext.");
    }

    private void WriteFile(string path, string content)
    {
        if (File.Exists(path))
            throw new InvalidOperationException($"Arquivo já existe (não sobrescrito): {path}");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine), Encoding.UTF8);
        _created.Add(Path.GetRelativePath(_root, path));
    }

    private static string MapCSharpType(string type) => type;

    private static string DefaultFor(string type) => type switch
    {
        "string" => " = string.Empty;",
        _ => string.Empty
    };
}
