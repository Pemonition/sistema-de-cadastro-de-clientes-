# Diagrama da solução

Componentes principais e como se relacionam. Renderiza nativamente no
GitHub (Mermaid).

```mermaid
flowchart TB
    subgraph App["SistemaCadastroClientes.App"]
        Program["Program.cs (composition root)"]
        Menu["MenuPrincipal"]
    end

    subgraph Application["SistemaCadastroClientes.Application"]
        CadastroSvc["ClienteCadastroService (RN01-RN09)"]
        ConsultaSvc["ClienteConsultaService"]
        CepSvc["CepConsultaService (contexto Strategy)"]
        CpfVal["CpfValidator"]
    end

    subgraph Domain["SistemaCadastroClientes.Domain"]
        ICepProvider(("ICepProvider"))
        IClienteRepository(("IClienteRepository"))
        Models["Cliente / Endereco / ResultadoConsultaCep"]
        Exceptions["Exceções de domínio"]
    end

    subgraph Infrastructure["SistemaCadastroClientes.Infrastructure"]
        Factory["CepProviderFactory"]
        ViaCep["ViaCepProvider"]
        BrasilApi["BrasilApiProvider"]
        OpenCep["OpenCepProvider"]
        Repo["SqliteClienteRepository"]
        DbInit["DatabaseInitializer"]
    end

    ExternalViaCep[["API ViaCEP"]]
    ExternalBrasilApi[["API BrasilAPI"]]
    ExternalOpenCep[["API OpenCEP"]]
    Sqlite[("clientes.db (SQLite)")]

    Program --> Menu
    Program -. monta/injeta .-> Factory
    Program -. monta/injeta .-> Repo
    Program -. monta/injeta .-> CadastroSvc
    Program -. monta/injeta .-> ConsultaSvc
    Program -. monta/injeta .-> CepSvc

    Menu --> CadastroSvc
    Menu --> ConsultaSvc
    Menu --> CepSvc
    Menu --> Factory

    CadastroSvc --> CpfVal
    CadastroSvc --> IClienteRepository
    ConsultaSvc --> IClienteRepository
    CepSvc --> ICepProvider

    Factory --> ViaCep
    Factory --> BrasilApi
    Factory --> OpenCep

    ViaCep -. implementa .-> ICepProvider
    BrasilApi -. implementa .-> ICepProvider
    OpenCep -. implementa .-> ICepProvider
    Repo -. implementa .-> IClienteRepository

    ViaCep --> ExternalViaCep
    BrasilApi --> ExternalBrasilApi
    OpenCep --> ExternalOpenCep
    Repo --> Sqlite
    DbInit --> Sqlite

    CadastroSvc -.-> Models
    CepSvc -.-> Models
    CadastroSvc -.-> Exceptions
```

## Leitura do diagrama

- **App** só conhece `Application` (regras) e, no `Program.cs`, monta as
  peças concretas de `Infrastructure` — é o único lugar que enxerga o
  sistema inteiro de uma vez (composition root).
- **Application** nunca importa nada de `Infrastructure`: fala só com as
  interfaces `ICepProvider` e `IClienteRepository`, definidas em `Domain`.
- **Infrastructure** implementa essas interfaces (Strategy para os
  provedores de CEP, Repository para a persistência), e é a única camada
  que sabe que existe HTTP ou SQLite por trás.
- As setas tracejadas "implementa" mostram por que adicionar o `OpenCepProvider`
  (Desafio Final) não exigiu tocar em `Domain` nem em `Application`: ele só
  precisou aparecer nesse desenho, ligado a `ICepProvider` e à `Factory`.
