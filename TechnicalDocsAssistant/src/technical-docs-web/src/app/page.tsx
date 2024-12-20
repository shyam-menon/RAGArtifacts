'use client';

import { useState } from 'react';
import UserStoryForm from '@/components/UserStoryForm';
import ArtifactDisplay from '@/components/ArtifactDisplay';
import Navigation from '@/components/Navigation';
import { TechnicalArtifact, UserStory } from '@/types';

export default function Home() {
    const [isLoading, setIsLoading] = useState(false);
    const [artifact, setArtifact] = useState<TechnicalArtifact | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (userStory: UserStory, artifactType: string) => {
        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch('http://localhost:5103/api/artifact/generate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    userStory,
                    artifactType,
                }),
            });

            if (!response.ok) {
                throw new Error('Failed to generate artifact');
            }

            const data = await response.json();
            setArtifact({
                id: new Date().toISOString(),
                type: artifactType as 'flowchart' | 'sequence' | 'testcases',
                content: data.content,
                userStoryId: userStory.title,
                createdAt: new Date().toISOString(),
            });
        } catch (err) {
            setError(err instanceof Error ? err.message : 'An error occurred');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Navigation />
            
            <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
                <div className="px-4 py-6 sm:px-0">
                    <div className="max-w-3xl mx-auto">
                        <h1 className="text-3xl font-bold text-gray-900 mb-8">
                            Generate Technical Documentation
                        </h1>

                        {error && (
                            <div className="mb-4 p-4 bg-red-50 border border-red-400 rounded-md">
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        )}

                        <div className="bg-white shadow sm:rounded-lg">
                            <div className="px-4 py-5 sm:p-6">
                                <UserStoryForm onSubmit={handleSubmit} isLoading={isLoading} />
                            </div>
                        </div>

                        <ArtifactDisplay artifact={artifact} />
                    </div>
                </div>
            </main>
        </div>
    );
}
