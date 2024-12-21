-- Enable the vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create the assets table
CREATE TABLE IF NOT EXISTS assets (
    id text PRIMARY KEY,
    title text NOT NULL,
    markdown_content text NOT NULL,
    content_vector vector(1536),  -- OpenAI Ada-2 embeddings are 1536 dimensions
    created timestamp with time zone DEFAULT timezone('utc'::text, now()) NOT NULL,
    modified timestamp with time zone DEFAULT timezone('utc'::text, now()) NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);

-- Create an index for similarity search
CREATE INDEX IF NOT EXISTS assets_content_vector_idx ON assets USING ivfflat (content_vector vector_cosine_ops);
