### Layout
// web/technical-docs-ui/src/app/layout.tsx
import { Inter } from 'next/font/google'
import '@/styles/globals.css'

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>
        <main className="flex min-h-screen flex-col">
          <Navbar />
          {children}
        </main>
      </body>
    </html>
  )
}

### Key pages
// web/technical-docs-ui/src/app/page.tsx
export default function Home() {
  return (
    <div className="container mx-auto">
      <h1>Technical Documentation Assistant</h1>
      <div className="grid grid-cols-2 gap-4">
        <Link href="/artifacts">Generate Artifacts</Link>
        <Link href="/search">Search Documentation</Link>
      </div>
    </div>
  )
}

// web/technical-docs-ui/src/app/artifacts/page.tsx
export default function ArtifactsPage() {
  return (
    <div className="container mx-auto">
      <ArtifactGenerationForm />
      <PreviewPanel />
    </div>
  )
}

// web/technical-docs-ui/src/app/search/page.tsx
export default function SearchPage() {
  return (
    <div className="container mx-auto">
      <DocumentSearch />
      <SearchResults />
    </div>
  )
}

### Key components
// web/technical-docs-ui/src/components/artifacts/ArtifactGenerationForm.tsx
import { UserStoryForm } from './UserStoryForm'
import { DiagramPreview } from './DiagramPreview'

export function ArtifactGenerationForm() {
  const handleSubmit = async (userStory: UserStory) => {
    // TODO: Call API to generate artifact
  }

  return (
    <div className="space-y-4">
      <UserStoryForm onSubmit={handleSubmit} />
      <DiagramPreview />
    </div>
  )
}

// web/technical-docs-ui/src/components/search/DocumentSearch.tsx
export function DocumentSearch() {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<SearchResult[]>([])

  const handleSearch = async () => {
    // TODO: Implement RAG-based search
  }

  return (
    <div className="space-y-4">
      <input 
        type="text"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className="w-full p-2 border rounded"
      />
      <button onClick={handleSearch}>Search</button>
      <SearchResultsList results={results} />
    </div>
  )
}

### Types
// web/technical-docs-ui/src/types/userStory.ts
export interface UserStory {
  title: string
  description: string
  actors: string[]
  preconditions: string[]
  postconditions: string[]
  mainFlow: string
  alternativeFlows: string[]
  businessRules: string[]
  dataRequirements: string[]
}

// web/technical-docs-ui/src/types/artifact.ts
export interface TechnicalArtifact {
  id: string
  type: 'flowchart' | 'sequence' | 'testcase'
  content: string
  userStoryId: string
  created: Date
  modified: Date
}

### API Client
// web/technical-docs-ui/src/lib/api.ts
export const api = {
  generateArtifact: async (userStory: UserStory): Promise<TechnicalArtifact> => {
    const response = await fetch('/api/artifacts', {
      method: 'POST',
      body: JSON.stringify(userStory)
    })
    return response.json()
  },

  searchDocuments: async (query: string): Promise<SearchResult[]> => {
    const response = await fetch(`/api/search?q=${encodeURIComponent(query)}`)
    return response.json()
  }
}

### Hooks
// web/technical-docs-ui/src/hooks/useArtifactGeneration.ts
export function useArtifactGeneration() {
  const [loading, setLoading] = useState(false)
  const [artifact, setArtifact] = useState<TechnicalArtifact | null>(null)

  const generateArtifact = async (userStory: UserStory) => {
    setLoading(true)
    try {
      const result = await api.generateArtifact(userStory)
      setArtifact(result)
    } finally {
      setLoading(false)
    }
  }

  return { generateArtifact, loading, artifact }
}

// web/technical-docs-ui/src/hooks/useDocumentSearch.ts
export function useDocumentSearch() {
  const [loading, setLoading] = useState(false)
  const [results, setResults] = useState<SearchResult[]>([])

  const search = async (query: string) => {
    setLoading(true)
    try {
      const searchResults = await api.searchDocuments(query)
      setResults(searchResults)
    } finally {
      setLoading(false)
    }
  }

  return { search, loading, results }
}

### Configuration
// web/technical-docs-ui/next.config.js
/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://localhost:5000/api/:path*'
      }
    ]
  }
}

module.exports = nextConfig