# Copilot Instructions

The github repo is shyam-menon/RAGArtifacts and the primary branch that I work off of is main

## Project Structure

- TechnicalDocsAssistant.Core is the main core library containing models, interfaces, and DTOs
- TechnicalDocsAssistant.Infrastructure implements services and data access components
- TechnicalDocsAssistant.API is the ASP.NET Core Web API that serves as the backend
- TechnicalDocsAssistant.SKPlugins contains Semantic Kernel plugins for AI integration
- technical-docs-web is a Next.js frontend application written in TypeScript
- tests directory contains unit and integration tests for the solution

## .NET

- Use .NET 7.0 or later for all .NET projects
- Follow async/await patterns throughout the codebase
- Use nullable reference types properly
- Implement proper disposal patterns for IDisposable resources
- Use dependency injection for all services

## React/Next.js

- Use TypeScript for all frontend code
- Follow React hooks patterns and functional components
- Use Tailwind CSS for styling
- Create component-specific styles when needed
- Ensure responsive design principles are followed
- Support both light and dark themes through Tailwind classes

## Code Style

- Use consistent naming conventions (PascalCase for C# public members, camelCase for private members and JavaScript/TypeScript)
- Use meaningful names for variables, methods, and classes
- Use file scoped namespaces in C# and are PascalCased
- Use interfaces for service contracts and put them in separate files
- Prefer async/await over direct Task handling
- Use latest C# features (records, pattern matching, etc.)
- Use latest TypeScript features when appropriate

## RAG Implementation

- Follow proper chunking strategies for documents
- Use OpenAI's embedding dimensions (1536) for vector storage
- Implement efficient vector search techniques
- Use proper indexing in Supabase for vector similarity search
- Monitor token usage for LLM calls
- Implement proper caching strategies where appropriate

## Component Structure

- Keep components small and focused
- Extract reusable logic into services
- Follow SOLID principles
- Maintain separation of concerns
- Use proper state management in React components

## Error Handling

- Implement proper error handling and logging
- Use try-catch blocks in event handlers
- Display user-friendly error messages
- Implement proper error boundaries in React

## Performance

- Optimize vector search with proper indexes
- Implement caching where appropriate
- Consider batch processing for large document sets
- Implement proper pagination for large result sets
- Monitor and optimize embedding generation performance

## Testing

- Add unit tests for core models and services
- Add integration tests for API endpoints
- Add UI component tests if applicable
- Test vector search performance with realistic data volumes

## Documentation

- Document API endpoints with XML comments for Swagger
- Add usage examples and sample requests/responses
- Update diagrams to reflect new components
- Document configuration requirements

## Security

- Implement proper authentication and authorization
- Validate all user inputs
- Secure API keys and sensitive configuration
- Use HTTPS for all communications
- Follow proper RLS policies in Supabase

## Supabase Integration

- Configure vector extension properly
- Use appropriate vector dimensions (1536 for OpenAI)
- Implement proper indexing strategies
- Configure authentication and authorization
- Implement proper RLS policies
- Monitor database performance

## File Organization

- Follow the existing project structure
- Keep related files together
- Use meaningful file names
- Group components by feature when possible

## Accessibility

- Use semantic HTML
- Include ARIA attributes where necessary
- Ensure keyboard navigation works
- Follow WCAG guidelines

---

**Last Updated**: May 11, 2025
**Version**: 1.0.0