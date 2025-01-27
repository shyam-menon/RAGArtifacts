'use client';

import { useEffect, useState } from 'react';
import UserStoryForm from '@/components/UserStoryForm';
import UserStoryList from '@/components/UserStoryList';
import EditUserStoryModal from '@/components/EditUserStoryModal';
import ArtifactDisplay from '@/components/ArtifactDisplay';
import Navigation from '@/components/Navigation';
import { UserStory, createUserStory, generateArtifact, getAllUserStories, updateUserStory, deleteUserStory } from '@/services/userStoryService';
import { TechnicalArtifact } from '@/types';
import AssetManagement from '@/components/AssetManagement';
import ChatInterface from '@/components/ChatInterface';

export default function Home() {
    const [isLoading, setIsLoading] = useState(false);
    const [artifact, setArtifact] = useState<TechnicalArtifact | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [currentUserStory, setCurrentUserStory] = useState<UserStory | null>(null);
    const [userStories, setUserStories] = useState<UserStory[]>([]);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [storyToEdit, setStoryToEdit] = useState<UserStory | null>(null);
    const [isAssetManagementOpen, setIsAssetManagementOpen] = useState(false);
    const [isChatOpen, setIsChatOpen] = useState(false);

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

    const handleEditUserStory = async (userStory: UserStory) => {
        setIsLoading(true);
        setError(null);

        try {
            if (!userStory.id) throw new Error('User story ID is required');
            await updateUserStory(userStory.id, userStory);
            await loadUserStories();
            setShowEditModal(false);
            setStoryToEdit(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to update user story');
        } finally {
            setIsLoading(false);
        }
    };

    const handleDeleteUserStory = async (userStory: UserStory) => {
        setIsLoading(true);
        setError(null);

        try {
            if (!userStory.id) throw new Error('User story ID is required');
            await deleteUserStory(userStory.id);
            await loadUserStories();
            if (currentUserStory?.id === userStory.id) {
                setCurrentUserStory(null);
                setArtifact(null);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to delete user story');
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
        <div className="min-h-screen bg-gray-100">
            <Navigation />
            <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
                {error && (
                    <div className="mb-4 p-4 text-red-700 bg-red-100 rounded">
                        {error}
                    </div>
                )}
                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-3xl font-bold">Technical Documentation Assistant</h1>
                    <div className="flex gap-4">
                        <button
                            onClick={() => setShowCreateForm(true)}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                        >
                            Create New Story
                        </button>
                        <button
                            onClick={() => setIsChatOpen(true)}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-indigo-700 bg-indigo-100 hover:bg-indigo-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                        >
                            Start Chat
                        </button>
                    </div>
                </div>

                <div className={`grid grid-cols-1 ${!showCreateForm ? 'md:grid-cols-2' : ''} gap-8`}>
                    <div className={showCreateForm ? 'col-span-full' : ''}>
                        {showCreateForm ? (
                            <div className="bg-white shadow sm:rounded-lg mb-6">
                                <div className="px-6 py-5 sm:p-6">
                                    <div className="flex justify-between items-center mb-6">
                                        <h2 className="text-xl font-medium text-gray-900">Create New User Story</h2>
                                        <button
                                            onClick={() => setShowCreateForm(false)}
                                            className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                                        >
                                            Cancel
                                        </button>
                                    </div>
                                    <UserStoryForm 
                                        onSubmit={handleCreateUserStory} 
                                        onCancel={() => setShowCreateForm(false)}
                                        isLoading={isLoading} 
                                    />
                                </div>
                            </div>
                        ) : (
                            <UserStoryList
                                userStories={userStories}
                                selectedStory={currentUserStory}
                                onSelect={handleSelectUserStory}
                                onEdit={(story) => {
                                    setStoryToEdit(story);
                                    setShowEditModal(true);
                                }}
                                onDelete={handleDeleteUserStory}
                            />
                        )}
                    </div>
                    <div className="space-y-6">
                        {currentUserStory && !showCreateForm && (
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
                        )}
                        {artifact && (
                            <ArtifactDisplay
                                artifact={artifact}
                                onClose={() => setArtifact(null)}
                            />
                        )}
                    </div>
                </div>
            </main>
            <EditUserStoryModal
                story={storyToEdit}
                isOpen={showEditModal}
                onClose={() => {
                    setShowEditModal(false);
                    setStoryToEdit(null);
                }}
                onSave={handleEditUserStory}
            />
            {isAssetManagementOpen && (
                <AssetManagement
                    isOpen={isAssetManagementOpen}
                    onClose={() => setIsAssetManagementOpen(false)}
                />
            )}
            
            {isChatOpen && (
                <ChatInterface
                    isOpen={isChatOpen}
                    onClose={() => setIsChatOpen(false)}
                />
            )}
        </div>
    );
}
