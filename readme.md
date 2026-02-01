# ClaudIA

Essa aplicação implementa um pequeno chatbot que tem como objetivo responder perguntas baseadas em conteúdo de uma empresa específica.

## Como executar

Para executar a aplicação, copie o arquivo `.env.example`, renomeie para `.env` e preencha com as API Keys necessárias.

Depois, execute na raiz do projeto

```bash
docker compose up -d --build
```

Quando o docker compose finalizar, a aplicação estará de pé e ouvindo na porta `8080`. 

Faça suas requisições para `http://localhost:8080/api/conversations/completions`, no payload preencha o campo `projectName` como `tesla_motors`.

Exemplo:

```bash
curl --location 'http://localhost:8080/api/conversations/completions' \
--header 'Content-Type: application/json' \
--data '{
    "helpdeskId": 123456,
    "projectName": "tesla_motors",
    "messages": [
        {
            "role": "USER",
            "content": "What can you tell me about Tesla?"
        }
    ]
}'
```

## A solução

O código foi escrito em C# com target para .NET 10. A aplicação conta com diferentes módulos, cada um com uma responsabilidade específica.

A interação entre os principais módulos está ilustrada na imagem abaixo:

### 

