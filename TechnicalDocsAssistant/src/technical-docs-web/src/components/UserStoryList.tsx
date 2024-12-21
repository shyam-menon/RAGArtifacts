import { UserStory } from '@/services/userStoryService';
import { useState } from 'react';

interface UserStoryListProps {
    userStories: UserStory[];
    selectedStory: UserStory | null;
    onSelect: (story: UserStory) => void;
    onEdit: (story: UserStory) => void;
    onDelete: (story: UserStory) => void;
}

export default function UserStoryList({ userStories, selectedStory, onSelect, onEdit, onDelete }: UserStoryListProps) {
    const [showDeleteConfirm, setShowDeleteConfirm] = useState<string | null>(null);

    const formatDate = (date: string | null | undefined) => {
        if (!date) return '';
        const d = new Date(date);
        if (isNaN(d.getTime())) return '';
        
        // Format the date with time
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        }).format(d);
    };

    const handleEdit = (story: UserStory, e: React.MouseEvent) => {
        e.stopPropagation();
        onEdit(story);
    };

    const handleDelete = (story: UserStory, e: React.MouseEvent) => {
        e.stopPropagation();
        setShowDeleteConfirm(story.id || null);
    };

    const confirmDelete = (story: UserStory, e: React.MouseEvent) => {
        e.stopPropagation();
        onDelete(story);
        setShowDeleteConfirm(null);
    };

    const cancelDelete = (e: React.MouseEvent) => {
        e.stopPropagation();
        setShowDeleteConfirm(null);
    };

    return (
        <div className="space-y-4">
            <h2 className="text-xl font-semibold">Available User Stories</h2>
            <p className="text-gray-600">Select a User Story</p>
            <div className="space-y-4">
                {userStories.map((story) => (
                    <div
                        key={story.id}
                        className={`relative p-4 border rounded-lg transition-colors ${
                            selectedStory?.id === story.id ? 'border-indigo-500 bg-indigo-50' : 'border-gray-200'
                        }`}
                        onClick={() => onSelect(story)}
                    >
                        <h3 className="text-lg font-medium mb-2">{story.title}</h3>
                        <p className="text-gray-600 mb-4">{story.description}</p>
                        <div className="text-sm text-gray-500 space-y-1">
                            <div>Created: {formatDate(story.createdAt)}</div>
                            {story.updatedAt && story.updatedAt !== story.createdAt && (
                                <div>Last edited: {formatDate(story.updatedAt)}</div>
                            )}
                        </div>
                        <div className="absolute top-2 right-2 flex gap-2">
                            <button
                                onClick={(e) => handleEdit(story, e)}
                                className="p-1 text-gray-500 hover:text-indigo-600 transition-colors"
                                title="Edit"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                                </svg>
                            </button>
                            {showDeleteConfirm === story.id ? (
                                <div className="absolute right-0 top-8 bg-white shadow-lg rounded-lg p-2 border z-10">
                                    <p className="text-sm mb-2">Delete this story?</p>
                                    <div className="flex gap-2">
                                        <button
                                            onClick={(e) => confirmDelete(story, e)}
                                            className="px-2 py-1 text-xs text-white bg-red-500 rounded hover:bg-red-600"
                                        >
                                            Yes
                                        </button>
                                        <button
                                            onClick={cancelDelete}
                                            className="px-2 py-1 text-xs text-gray-700 bg-gray-100 rounded hover:bg-gray-200"
                                        >
                                            No
                                        </button>
                                    </div>
                                </div>
                            ) : (
                                <button
                                    onClick={(e) => handleDelete(story, e)}
                                    className="p-1 text-gray-500 hover:text-red-600 transition-colors"
                                    title="Delete"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                        <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                                    </svg>
                                </button>
                            )}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}
