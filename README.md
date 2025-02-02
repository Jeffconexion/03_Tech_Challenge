# Tech Challenger - Microsserviços e Mensageria

# Introdução

Na fase anterior do Tech Challenge, desenvolvemos um aplicativo .NET para cadastro de contatos regionais, com funcionalidades de adicionar, consultar, atualizar e excluir contatos, utilizando Entity Framework Core ou Dapper para persistência de dados e implementando validações de dados. Além da criação de CI/CD e monitoramento.

Nesta terceira fase, vamos aprimorar o projeto através de microsserviços e comunicação por mensageria. Mantendo também o funcionamento da fase anterior.

![Descrição da imagem](LocalFriendzApi/imgs/requisitos.png)

# Objetivos

- **Arquitetura de Microsserviços:**
    - refatorar o aplicativo existente em um
    conjunto de microsserviços em que tenhamos as funcionalidades
    separadas por contexto. Ex.: o cadastro ser um microsserviço que
    envia dados para um outro microsserviço persistir os dados.
- **Comunicação Assíncrona com Mensageria:**
    - implementar comunicação assíncrona usando RabbitMQ para eventos entre os
    microsserviços. Ex.: adicionar o RabbitMQ para comunicação entre os
    microsserviços.

# Tecnologias Utilizadas:

- **.NET 8**: Framework para construção da Minimal API.
- **C#**: Linguagem de programação usada no desenvolvimento do projeto.
- **Entity Framework**: ORM (Object-Relational Mapping) utilizado para interagir com o banco de dados.
- **xUnit**: Framework de testes utilizado para realizar testes unitários.
- **Postegress**: Banco de dados relacional usado para armazenar os dados da aplicação.
- **Prometheus**: Ferramenta projetada para coletar, armazenar e consultar métricas de sistemas e serviços..
- **Grafana**: Plataforma de análise e visualização de código aberto que permite criar dashboards dinâmicos e interativos.

# Vídeo explicativo

- [Parte 1 - CI e CD (Branch Master)](https://drive.google.com/file/d/1IRKbsJnJ2XN0EcXyjbgSnHGPer4UXDHa/view)
- [Parte 2 - Monitoramento (Branch feature/grafana )](https://drive.google.com/file/d/1D0Ft5DvXD_T-1yJRdh8nVR3w2bgswGPW/view)

# Documentação

- [WorkFlow](https://horse-neon-79c.notion.site/Workflow-GitHub-Actions-e0cf8a925de945bc89acc7a61de6ab87?pvs=4)
- [DockerFile](https://horse-neon-79c.notion.site/DockerFile-Configura-es-3d917ef39a994f68b4ecd02f163b17a8?pvs=4)
- [Prometheus](https://horse-neon-79c.notion.site/Prometheus-Configura-es-dff855874cb14e34ab307de9f4cdb59a?pvs=4)
- [Grafana](https://horse-neon-79c.notion.site/Grafana-Configura-es-579faa08d53942d894ab27e6b755a035?pvs=4)

# **Checklist de Conclusão de Tarefas**

1. Microsserviços
  - [ ]  Microsserviços para cadastro de contato.(Consumer)
2. Padrões de Comunicações
  - [ ]  Pesquisar sobre padrões de comunicação e procurar aplicar.
3. Mensageria com RaabbitMq
  - [ ]  Producer cadastrar o contato na fila
4. Microsserviços
  - [ ]  Refatorações gerais e pipeline funcionando.
