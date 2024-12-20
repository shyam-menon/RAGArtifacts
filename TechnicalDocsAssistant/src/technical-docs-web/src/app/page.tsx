'use client';

import { useEffect, useState } from 'react';
import UserStoryForm from '@/components/UserStoryForm';
import UserStoryList from '@/components/UserStoryList';
import ArtifactDisplay from '@/components/ArtifactDisplay';
import Navigation from '@/components/Navigation';
import { UserStory, createUserStory, generateArtifact, getAllUserStories } from '@/services/userStoryService';
import { TechnicalArtifact } from '@/types';

export default function Home() {
    const [isLoading, setIsLoading] = useState(false);
    const [artifact, setArtifact] = useState<TechnicalArtifact | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [currentUserStory, setCurrentUserStory] = useState<UserStory | null>(null);
    const [userStories, setUserStories] = useState<UserStory[]>([]);
    const [showCreateForm, setShowCreateForm] = useState(false);

    useEffect(() => {
        loadUserStories();
    }, []);

    const loadUserStories = async () => {
        try {
            const stories = await getAllUserStories();
            setUserStories(stories);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load user stories');
        }
    };

    const handleCreateUserStory = async (userStory: UserStory) => {
        setIsLoading(true);
        setError(null);

        try {
            const createdStory = await createUserStory(userStory);
            await loadUserStories();
            setCurrentUserStory(createdStory);
            setShowCreateForm(false);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to create user story');
        } finally {
            setIsLoading(false);
        }
    };

    const handleGenerateArtifact = async (artifactType: string) => {
        if (!currentUserStory?.id) return;

        setIsLoading(true);
        setError(null);

        try {
            const artifactResponse = await generateArtifact(currentUserStory.id, artifactType);
            
            // For test cases, convert markdown to HTML
            let content = artifactResponse.content;
            if (artifactType === 'testcases') {
                content = content
                    .replace(/^# (.*$)/gm, '<h1>$1</h1>')
                    .replace(/^## (.*$)/gm, '<h2>$1</h2>')
                    .replace(/^### (.*$)/gm, '<h3>$1</h3>')
                    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                    .replace(/\*(.*?)\*/g, '<em>$1</em>')
                    .replace(/^- (.*$)/gm, '<li>$1</li>')
                    .replace(/\n\n/g, '<br/>');
            }

            setArtifact({
                id: currentUserStory.id,
                type: artifactType as 'flowchart' | 'sequence' | 'testcases',
                content: content,
                userStoryId: currentUserStory.id,
                createdAt: new Date().toISOString(),
            });
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to generate artifact');
        } finally {
            setIsLoading(false);
        }
    };

    const handleSelectUserStory = (story: UserStory) => {
        setCurrentUserStory(story);
        setArtifact(null);
        setError(null);
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <Navigation />
            
            <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
                <div className="px-4 py-6 sm:px-0">
                    <div className="max-w-3xl mx-auto">
                        <div className="flex justify-between items-center mb-8">
                            <h1 className="text-3xl font-bold text-gray-900">
                                Technical Documentation Assistant
                            </h1>
                            <button
                                onClick={() => setShowCreateForm(true)}
                                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                            >
                                Create New Story
                            </button>
                        </div>

                        {error && (
                            <div className="mb-4 p-4 bg-red-50 border border-red-400 rounded-md">
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        )}

                        {showCreateForm ? (
                            <div className="bg-white shadow sm:rounded-lg mb-6">
                                <div className="px-4 py-5 sm:p-6">
                                    <div className="flex justify-between items-center mb-4">
                                        <h2 className="text-lg font-medium text-gray-900">Create New User Story</h2>
                                        <button
                                            onClick={() => setShowCreateForm(false)}
                                            className="text-sm text-gray-500 hover:text-gray-700"
                                        >
                                            Cancel
                                        </button>
                                    </div>
                                    <UserStoryForm onSubmit={handleCreateUserStory} isLoading={isLoading} />
                                </div>
                            </div>
                        ) : (
                            <div className="bg-white shadow sm:rounded-lg mb-6">
                                <div className="px-4 py-5 sm:p-6">
                                    <h2 className="text-lg font-medium text-gray-900 mb-4">Available User Stories</h2>
                                    {userStories.length === 0 ? (
                                        <p className="text-gray-500">No user stories yet. Create your first one!</p>
                                    ) : (
                                        <UserStoryList
                                            userStories={userStories}
                                            selectedStory={currentUserStory}
                                            onSelect={handleSelectUserStory}
                                        />
                                    )}
                                </div>
                            </div>
                        )}

                        {currentUserStory && !showCreateForm && (
                            <div className="space-y-6">
                                <div className="bg-white shadow sm:rounded-lg">
                                    <div className="px-4 py-5 sm:p-6">
                                        <h2 className="text-lg font-medium text-gray-900 mb-4">Generate Artifact</h2>
                                        <div className="space-y-4">
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700">Select Artifact Type</label>
                                                <select
                                                    onChange={(e) => handleGenerateArtifact(e.target.value)}
                                                    disabled={isLoading}
                                                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                                                >
                                                    <option value="">Select type...</option>
                                                    <option value="flowchart">Flowchart</option>
                                                    <option value="sequence">Sequence Diagram</option>
                                                    <option value="testcases">Test Cases</option>
                                                </select>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                {artifact && (
                                    <div className="bg-white shadow sm:rounded-lg">
                                        <div className="px-4 py-5 sm:p-6">
                                            <ArtifactDisplay artifact={artifact} />
                                        </div>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                </div>
            </main>
        </div>
    );
}
