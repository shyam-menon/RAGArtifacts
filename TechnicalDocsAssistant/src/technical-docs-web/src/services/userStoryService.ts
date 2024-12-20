export interface UserStory {
    id?: string;
    title: string;
    description: string;
    actors: string[];
    preconditions: string[];
    postconditions: string[];
    mainFlow: string[];
    alternativeFlows: string[];
    businessRules: string[];
    dataRequirements: string[];
    nonFunctionalRequirements: string[];
    assumptions: string[];
    createdAt?: string;
    updatedAt?: string;
}

const API_BASE_URL = 'http://localhost:5103/api';

export async function createUserStory(userStory: UserStory): Promise<UserStory> {
    const response = await fetch(`${API_BASE_URL}/UserStory`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userStory),
    });

    if (!response.ok) {
        throw new Error('Failed to create user story');
    }

    return response.json();
}

export async function getUserStoryById(id: string): Promise<UserStory> {
    const response = await fetch(`${API_BASE_URL}/UserStory/${id}`);

    if (!response.ok) {
        throw new Error('Failed to fetch user story');
    }

    return response.json();
}

export async function getAllUserStories(): Promise<UserStory[]> {
    const response = await fetch(`${API_BASE_URL}/UserStory`);

    if (!response.ok) {
        throw new Error('Failed to fetch user stories');
    }

    return response.json();
}

export async function updateUserStory(id: string, userStory: UserStory): Promise<UserStory> {
    const response = await fetch(`${API_BASE_URL}/UserStory/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userStory),
    });

    if (!response.ok) {
        throw new Error('Failed to update user story');
    }

    return response.json();
}

export async function deleteUserStory(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/UserStory/${id}`, {
        method: 'DELETE',
    });

    if (!response.ok) {
        throw new Error('Failed to delete user story');
    }
}

export async function generateArtifact(userStoryId: string, artifactType: string): Promise<any> {
    const response = await fetch(`${API_BASE_URL}/artifact/generate`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            userStoryId,
            artifactType
        }),
    });

    if (!response.ok) {
        throw new Error('Failed to generate artifact');
    }

    return response.json();
}
