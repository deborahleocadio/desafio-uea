# Como Rodar o Projeto

Este projeto é dividido em duas partes principais: o **backend** e o **frontend**. Siga as instruções abaixo para configurar e executar cada parte.

## Pré-requisitos

Certifique-se de ter as seguintes ferramentas instaladas em sua máquina:

- **Node.js** (versão 16 ou superior)
- **Angular CLI** (para o frontend)
- **.NET SDK** (versão 9.0 ou superior para o backend)
- **Git** (para controle de versão)

---

## Rodando o Backend

1. Navegue até o diretório do backend:

   ```bash
   cd backend
   ```

2. Restaure as dependências do projeto:

   ```bash
   dotnet restore
   ```

3. Execute o servidor de desenvolvimento:

   ```bash
   dotnet run
   ```

4. O backend estará disponível em `http://localhost:5000` por padrão.

---

## Rodando o Frontend

1. Navegue até o diretório do frontend:

   ```bash
   cd frontend
   ```

2. Instale as dependências do projeto:

   ```bash
   npm install
   ```

3. Execute o servidor de desenvolvimento:

   ```bash
   ng serve
   ```

4. O frontend estará disponível em `http://localhost:4200` por padrão.

---

## Observações

- Certifique-se de que o backend esteja rodando antes de acessar o frontend.
- Para mais informações, consulte a documentação específica de cada parte do projeto.
