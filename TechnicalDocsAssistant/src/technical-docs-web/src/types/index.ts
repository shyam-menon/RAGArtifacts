export interface UserStory {
    title: string;
    description: string;
    actors: string[];
    preconditions: string[];
    mainFlow: string;
    alternativeFlows: string[];
    businessRules: string[];
}

export interface TechnicalArtifact {
    id: string;
    type: 'flowchart' | 'sequence' | 'testcases';
    content: string;
    userStoryId: string;
    createdAt: string;
    updatedAt?: string;
    generatedBy?: string;
    version?: string;
}

export interface GenerateArtifactRequest {
    userStory: UserStory;
    artifactType: 'flowchart' | 'sequence' | 'testcases';
}
