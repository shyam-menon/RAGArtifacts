-- Create user_stories table
CREATE TABLE IF NOT EXISTS user_stories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title TEXT NOT NULL,
    description TEXT NOT NULL,
    actors TEXT[] DEFAULT '{}',
    preconditions TEXT[] DEFAULT '{}',
    postconditions TEXT[] DEFAULT '{}',
    main_flow TEXT[] DEFAULT '{}',
    alternative_flows TEXT[] DEFAULT '{}',
    business_rules TEXT[] DEFAULT '{}',
    data_requirements TEXT[] DEFAULT '{}',
    non_functional_requirements TEXT[] DEFAULT '{}',
    assumptions TEXT[] DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create trigger to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_user_stories_updated_at
    BEFORE UPDATE ON user_stories
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
