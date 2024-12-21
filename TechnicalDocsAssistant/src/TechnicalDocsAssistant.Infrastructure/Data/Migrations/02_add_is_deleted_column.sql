-- Add is_deleted column to assets table
ALTER TABLE IF EXISTS assets 
ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;

-- Create function for similarity search
CREATE OR REPLACE FUNCTION search_assets_by_similarity(
    query_embedding vector(1536),
    match_threshold float,
    match_count int
)
RETURNS TABLE (
    id uuid,
    title VARCHAR(200),
    markdown_content TEXT,
    content_vector vector(1536),
    created_at TIMESTAMP WITH TIME ZONE,
    modified_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN,
    similarity float
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        a.id,
        a.title,
        a.markdown_content,
        a.content_vector,
        a.created_at,
        a.modified_at,
        a.is_deleted,
        1 - (a.content_vector <=> query_embedding) as similarity
    FROM assets a
    WHERE a.is_deleted = FALSE
    AND 1 - (a.content_vector <=> query_embedding) > match_threshold
    ORDER BY a.content_vector <=> query_embedding
    LIMIT match_count;
END;
$$;
