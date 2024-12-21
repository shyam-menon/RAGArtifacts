CREATE OR REPLACE FUNCTION search_assets_by_similarity(
    match_count INT,
    match_threshold FLOAT,
    query_embedding TEXT
)
RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    query_vector vector;
BEGIN
    -- Convert the query embedding from text to vector
    query_vector := query_embedding::vector;

    RETURN (
        SELECT json_agg(row_to_json(t))
        FROM (
            SELECT 
                a.id,
                a.title,
                a.markdown_content,
                a.content_vector::text as content_vector,
                a.created,
                a.modified,
                a.is_deleted,
                1 - (a.content_vector <=> query_vector) as similarity
            FROM assets a
            WHERE 1 - (a.content_vector <=> query_vector) > match_threshold
                AND NOT a.is_deleted
            ORDER BY similarity DESC
            LIMIT match_count
        ) t
    );
END;
$$;
