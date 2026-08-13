# Sistema de Cadastro de Clientes — Ardena Soluções

Projeto prático da disciplina de C#, desenvolvido a partir da especificação
`ARD-TI-ESP-2026-014`. Aplicação de console que cadastra clientes,
preenche o endereço automaticamente a partir do CEP (usando um provedor
externo escolhido em tempo de execução) e persiste tudo em SQLite.

## Sumário

- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Como executar](#como-executar)
- [Como configurar o banco](#como-configurar-o-banco)
- [Configuração das APIs de CEP](#configuração-das-apis-de-cep)
- [Como usar](#como-usar)
- [Arquitetura e justificativa](#arquitetura-e-justificativa)
- [Como adicionar um novo provedor de CEP](#como-adicionar-um-novo-provedor-de-cep)

## Tecnologias utilizadas

- **.NET 10 / C#** — console app (`SistemaCadastroClientes.App`)
- **Microsoft.Data.Sqlite** — persistência, via ADO.NET puro (sem ORM)
- **System.Net.Http.Json** — consumo das APIs de CEP
- **ViaCEP**, **BrasilAPI** e **OpenCEP** — provedores de consulta de CEP

## Como executar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/SistemaCadastroClientes.App
```

O menu é interativo: primeiro escolha um provedor de CEP, depois use as
opções de cadastro/consulta/troca de provedor.

## Como configurar o banco

Não é preciso nenhuma configuração manual. Na primeira execução, o
`DatabaseInitializer` cria automaticamente o arquivo `clientes.db` (SQLite)
ao lado do executável, com a tabela `Clientes`. O script equivalente, para
inspeção manual, está em [`database/schema.sql`](database/schema.sql).

## Configuração das APIs de CEP

Nenhuma chave de API é necessária — ViaCEP, BrasilAPI e OpenCEP são
públicas e gratuitas. É necessário apenas acesso à internet no momento da
consulta de um CEP.

## Como usar

1. Ao iniciar, escolha o provedor de CEP (ViaCEP, BrasilAPI ou OpenCEP).
2. `1) Cadastrar cliente` — informe nome, CPF e CEP; o endereço retornado
   pela API é exibido, você completa número/complemento e confirma. Só
   depois da confirmação o cliente é gravado no banco.
3. `2) Consultar clientes cadastrados` — lista os clientes salvos.
4. `3) Trocar provedor de CEP` — troca a estratégia de consulta em tempo de
   execução; as próximas consultas usam o novo provedor.
5. `0) Sair`.

Situações tratadas: CPF ausente/inválido/duplicado, CEP ausente/não
encontrado, falha de comunicação com a API e falha de acesso ao banco —
em nenhum desses casos a aplicação encerra inesperadamente.

## Arquitetura e justificativa

O projeto é dividido em 4 projetos .NET, cada um com uma responsabilidade:

```
SistemaCadastroClientes.Domain          modelos e abstrações — não depende de mais nada
SistemaCadastroClientes.Application     regras de negócio (RN01-RN12) — depende só de Domain
SistemaCadastroClientes.Infrastructure  APIs de CEP + SQLite — depende só de Domain
SistemaCadastroClientes.App             console + composition root — depende de todos
```

A regra é sempre a mesma: **Domain não depende de nada, e tudo depende de
Domain através de interface** (`ICepProvider`, `IClienteRepository`). Isso é
o que torna a "Independência do provedor" (RN11) e a manutenibilidade
(seção 16 da especificação) possíveis — trocar SQLite por outro banco, ou
adicionar/remover um provedor de CEP, nunca exige tocar em
`ClienteCadastroService` ou no menu.

### Padrões utilizados e por quê

| Padrão | Onde | Por quê |
|---|---|---|
| **Strategy** | `ICepProvider` + `ViaCepProvider`/`BrasilApiProvider`/`OpenCepProvider` | O algoritmo "consultar um CEP" varia por provedor, mas a assinatura é sempre a mesma. `CepConsultaService` é o *contexto* do Strategy: guarda o provedor atual e delega a chamada, permitindo trocá-lo em runtime (RN10) sem `if/else` espalhado pelo código. |
| **Adapter** | Classes DTO privadas dentro de cada `*CepProvider` (`ViaCepResponse`, `BrasilApiResponse`, `OpenCepResponse`) | Cada API externa tem um formato de JSON diferente (nomes de campo, forma de sinalizar "não encontrado"). Cada provedor adapta sua resposta específica para o `Endereco` comum do domínio — o resto do sistema nunca vê o JSON original. |
| **Factory** | `CepProviderFactory` | Centraliza a lista de provedores disponíveis e a resolução por índice/nome, para que o menu não precise saber como cada provedor é construído. |
| **Repository** | `IClienteRepository` / `SqliteClienteRepository` | Isola toda a lógica de SQL. A camada de aplicação só sabe "adicionar", "existe CPF" e "listar" — nunca viu uma `SqliteConnection`. |
| **SOLID — SRP** | Separação entre `MenuPrincipal` (I/O), `ClienteCadastroService` (regras), `SqliteClienteRepository` (persistência) e cada `*CepProvider` (integração HTTP) | Nenhuma classe mistura HTTP + regra de negócio + acesso a banco, exatamente o que a seção 14 da especificação proíbe. |
| **SOLID — DIP** | `ClienteCadastroService` e `CepConsultaService` dependem só de interfaces de `Domain` | Permite substituir qualquer implementação concreta sem alterar quem a consome. |
| **Composition root manual** | `Program.cs` | Em vez de um container de DI, a ligação entre interfaces e implementações concretas é feita manualmente, num único lugar — deixa explícito, para fins didáticos, quem depende de quem. |

Deliberadamente **não** foram usados Singleton (não há estado global que
precise de instância única — o `HttpClient` é compartilhado por injeção de
dependência simples, não por um Singleton estático) nem uma camada de ORM
(EF Core seria razoável em produção, mas ADO.NET puro deixa visível o que o
Repository está escondendo).

### Fluxo de consulta de CEP

```
Usuário → MenuPrincipal → CepConsultaService (Strategy/contexto)
                                 │
                                 ▼
                    ICepProvider escolhido (Strategy concreta)
                                 │
                          HTTP GET na API externa
                                 │
                                 ▼
                  DTO específico do provedor (Adapter)
                                 │
                                 ▼
                    Endereco (representação comum)
```

## Como adicionar um novo provedor de CEP

Este é o cenário do "Desafio Arquitetural" (seção 19) e do "Desafio Final"
(seção 24) da especificação — e o motivo de toda a estrutura acima. Passos:

1. Criar uma classe em `SistemaCadastroClientes.Infrastructure/CepProviders/`
   que implemente `ICepProvider` (foi assim que `OpenCepProvider` foi
   adicionado).
2. Nela, mapear o formato de resposta específico do novo serviço para
   `Endereco` — inclusive tratando como esse serviço sinaliza "CEP não
   encontrado" (nem toda API usa HTTP 404; o ViaCEP, por exemplo, usa
   `"erro": true` com status 200).
3. Registrar uma linha em `Program.cs`, dentro do array passado a
   `CepProviderFactory`.

Nenhuma outra classe do sistema — `ClienteCadastroService`, `MenuPrincipal`,
`SqliteClienteRepository` — precisa ser tocada. Isso é o que RN11/RN12
pedem, e é a prova prática de que a "independência do provedor" não é
apenas uma frase na especificação.
