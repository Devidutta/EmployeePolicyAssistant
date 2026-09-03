# Employee Policy Assistant

.NET 10 console app that answers natural-language employee policy questions
using Retrieval Augmented Generation (RAG), per
`Assignment-04-Employee-Policy-Assistant-Requirements.pdf`.

## RAG Flow

**Indexing (once, at startup)**
1. `DocumentLoaderService` reads `Data/EmployeePolicies.txt` and splits it into
   section-based chunks (one per `## Heading`).
2. `EmbeddingService` calls the embedding model (`text-embedding-3-small`) to
   turn each chunk's text into a vector.
3. Chunks and their vectors are held in memory as `VectorRecord`s — no
   external vector database.

**Per question**
1. The employee's question is embedded with the same embedding model.
2. `SimilarityService.CosineSimilarity` scores every indexed chunk against the
   question vector.
3. `RagService.AskAsync` ranks chunks by score and keeps the Top-K
   (`Rag:TopK` in config, default 3) — this is **Retrieval**.
4. The retrieved chunk text is concatenated into a context block —
   **Augmentation**.
5. `LlmService` sends the context + question to the chat model
   (`openai.gpt-5-mini`) with instructions to answer only from the supplied
   context, or say there isn't enough information — **Generation**.

Retrieval and generation are handled by separate services; there is no
if/else mapping of known questions to policy sections and no hard-coded
answers — every response is grounded in whatever the semantic search
actually retrieves.

## Setup

1. Copy your real key into `appsettings.Local.json` (git-ignored, never
   committed):
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-..."
     }
   }
   ```
2. `appsettings.json` holds the non-secret template:
   ```json
   {
     "OpenAI": {
       "ApiKey": "",
       "Endpoint": "https://api.openai.com/v1",
       "EmbeddingModel": "text-embedding-3-small",
       "ChatModel": "openai.gpt-5-mini"
     },
     "Rag": {
       "TopK": 3,
       "PolicyFilePath": "Data/EmployeePolicies.txt"
     }
   }
   ```
3. Run:
   ```
   dotnet run --project EmployeePolicyAssistant
   ```
4. Ask a policy question, e.g. "Can I work from home?" or "What is the
   process for planned leave?". The console shows the retrieved sections and
   their similarity scores, then the generated answer.
5. To verify grounding, ask something not covered by the policy file, e.g.
   "What is the employee cafeteria menu for tomorrow?" — the assistant
   should say it doesn't have enough information rather than inventing an
   answer.
6. Type `exit` (or press Enter on a blank line) to quit.

## Structure

- `Data/EmployeePolicies.txt` — fictional policy knowledge base, one
  `## Heading` per section.
- `Models/OpenAIOptions.cs` — bound config (API key, endpoint, embedding &
  chat model names).
- `Models/RagOptions.cs` — bound config (Top-K, policy file path).
- `Models/VectorRecord.cs` — indexed chunk: title, content, vector.
- `Models/SearchResult.cs` — a ranked chunk with its similarity score.
- `Services/DocumentLoaderService.cs` — loads and splits the policy file into
  section chunks.
- `Services/EmbeddingService.cs` — calls the OpenAI embedding endpoint.
- `Services/SimilarityService.cs` — cosine similarity between two vectors.
- `Services/LlmService.cs` — calls the chat model to generate a grounded
  answer from context.
- `Services/RagService.cs` — orchestrates indexing, retrieval, augmentation,
  and generation.
- `Program.cs` — indexes the policy file once at startup, then loops
  accepting questions until the user exits.
