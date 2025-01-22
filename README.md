# RAGArtifacts

A tool for managing and analyzing artifacts in Retrieval Augmented Generation (RAG) systems.

## Overview

RAGArtifacts is designed to help developers and data scientists manage, analyze, and optimize their RAG-based applications. It provides tools for document processing, embedding management, and retrieval optimization.

## Features

- Document Processing and Management
- Embedding Generation and Storage
- Retrieval Quality Analysis
- Performance Metrics and Monitoring
- Integration with Popular LLM Frameworks
- Supabase Integration for Vector Storage and Authentication

## Prerequisites

- .NET 7.0 SDK or later
- Node.js 16.x or later
- Windows 11 64-bit (or compatible OS)
- Supabase Account (for vector storage and authentication)
- Required Python packages (see `requirements.txt`)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/shyam-menon/RAGArtifacts.git
cd RAGArtifacts
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Set up Supabase:
   - Create a new project in Supabase
   - Enable Vector Storage extension in SQL Editor:
```sql
create extension if not exists vector;

-- Create a table for your embeddings
create table documents (
  id bigserial primary key,
  content text,
  embedding vector(1536)
);

-- Create a hypertable for better performance
create index on documents 
using ivfflat (embedding vector_cosine_ops)
with (lists = 100);
```

## Running the Application

### Backend Setup (.NET Web API)

1. Navigate to the backend directory:
```bash
cd src/RAGArtifacts.Api
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Set up environment variables:
   - Update `appsettings.json` and `appsettings.Development.json` with your configuration
   - Or use User Secrets for development (recommended):
```bash
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "your_api_key"
dotnet user-secrets set "Supabase:Url" "your_supabase_project_url"
dotnet user-secrets set "Supabase:AnonKey" "your_supabase_anon_key"
dotnet user-secrets set "Supabase:ServiceRoleKey" "your_supabase_service_role_key"
```

4. Build and run the application:
```bash
dotnet build
dotnet run
```

The backend API will be available at `https://localhost:7000` (HTTPS) or `http://localhost:5000` (HTTP)

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install frontend dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

The frontend application will be available at `http://localhost:3000`

### API Documentation

Once the backend is running, you can access the API documentation at:
- Swagger UI: `https://localhost:7000/swagger`
- API Documentation: `https://localhost:7000/api-docs`

### Configuration

#### Backend (appsettings.json)
```json
{
  "OpenAI": {
    "ApiKey": "your_api_key"
  },
  "Supabase": {
    "Url": "your_supabase_project_url",
    "AnonKey": "your_supabase_anon_key",
    "ServiceRoleKey": "your_supabase_service_role_key"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

#### Frontend (.env)
```
REACT_APP_API_URL=https://localhost:7000
REACT_APP_SUPABASE_URL=your_supabase_project_url
REACT_APP_SUPABASE_ANON_KEY=your_supabase_anon_key
```

### Supabase Setup Details

#### Authentication
The project uses Supabase Authentication. To enable authentication:

1. Go to Authentication settings in your Supabase dashboard
2. Configure your preferred auth providers (Email, Google, GitHub, etc.)
3. Update the Site URL and redirect URLs in Authentication settings:
   - Site URL: `http://localhost:3000`
   - Redirect URLs: 
     - `http://localhost:3000/auth/callback`
     - `http://localhost:3000/auth/reset-password`

#### Vector Storage
The project uses Supabase's pgvector extension for storing and querying embeddings:

1. Vector dimensions are set to 1536 (OpenAI's embedding size)
2. Indexing is optimized for cosine similarity search
3. The documents table includes:
   - Document content
   - Embedding vectors
   - Metadata fields for better organization

For optimal performance, consider:
- Regular VACUUM operations
- Monitoring index usage
- Adjusting number of IVFFlat lists based on your data size

## Project Structure

```
RAGArtifacts/
├── src/               # Source code
├── tests/             # Test files
├── docs/              # Documentation
├── examples/          # Example usage
└── requirements.txt   # Project dependencies
```

```plantuml
@startuml Technical Docs Assistant Architecture

skinparam componentStyle uml2
skinparam linetype ortho

package "Presentation Layer" {
    [Web UI] as UI
}

package "API Layer" {
    [AssetsController] as AC
    [ChatController] as CC
    [UserStoryController] as USC
    [ArtifactController] as ARC
}

package "Core Layer" {
    interface "IAssetService" as IAS
    interface "IChatService" as ICS
    interface "IUserStoryService" as IUSS
    
    package "DTOs" {
        [AssetDTO]
        [ChatRequest]
        [UserStoryDTO]
        [GenerateArtifactRequest]
    }
    
    package "Models" {
        [Asset]
        [ChatResponse]
        [UserStory]
        [Artifact]
    }
}

package "Infrastructure Layer" {
    [AssetService] as AS
    [SimpleChatService] as SCS
    [UserStoryService] as USS
    [ArtifactGenerationPlugin] as AGP
    [InputAnalyzer]
    [SK Agents] as SK
}

' UI to Controller interactions
UI --> AC : HTTP Requests
UI --> CC : HTTP Requests
UI --> USC : HTTP Requests
UI --> ARC : HTTP Requests

' Controller to Interface interactions
AC ..> IAS : Depends on
CC ..> ICS : Depends on
USC ..> IUSS : Depends on
ARC ..> IUSS : Depends on

' Service implementations
AS ..|> IAS : Implements
SCS ..|> ICS : Implements
USS ..|> IUSS : Implements

' Service dependencies
AS --> SK : Uses
SCS --> SK : Uses
AGP --> SK : Uses
AS --> InputAnalyzer : Uses
SCS --> InputAnalyzer : Uses

' Data flow
AC --> AssetDTO : Uses
CC --> ChatRequest : Uses
USC --> UserStoryDTO : Uses
ARC --> GenerateArtifactRequest : Uses

AS --> Asset : Manages
SCS --> ChatResponse : Produces
USS --> UserStory : Manages
AGP --> Artifact : Generates

@enduml


## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions and support, please open an issue in the GitHub repository.

---
Last Updated: January 21, 2025
