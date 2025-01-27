import { UserStory } from '@/services/userStoryService';
import { useState, useEffect, useCallback } from 'react';

interface EditUserStoryModalProps {
    story: UserStory | null;
    isOpen: boolean;
    onClose: () => void;
    onSave: (story: UserStory) => void;
}

export default function EditUserStoryModal({ story, isOpen, onClose, onSave }: EditUserStoryModalProps) {
    const [formData, setFormData] = useState<UserStory>({
        title: '',
        description: '',
        actors: [],
        preconditions: [],
        postconditions: [],
        mainFlow: [],
        alternativeFlows: [],
        businessRules: [],
        dataRequirements: [],
        nonFunctionalRequirements: [],
        assumptions: []
    });

    useEffect(() => {
        if (story) {
            setFormData(story);
        }
    }, [story]);

    // Handle ESC key press
    const handleEscapeKey = useCallback((event: KeyboardEvent) => {
        if (event.key === 'Escape' && isOpen) {
            onClose();
        }
    }, [isOpen, onClose]);

    useEffect(() => {
        // Add event listener for ESC key
        document.addEventListener('keydown', handleEscapeKey);
        return () => {
            document.removeEventListener('keydown', handleEscapeKey);
        };
    }, [handleEscapeKey]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSave({ ...formData, id: story?.id });
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleListItemChange = (field: keyof UserStory, index: number, value: string) => {
        const array = [...(formData[field] as string[])];
        array[index] = value;
        setFormData({ ...formData, [field]: array });
    };

    const handleAddListItem = (field: keyof UserStory) => {
        setFormData({
            ...formData,
            [field]: [...(formData[field] as string[]), '']
        });
    };

    const handleRemoveListItem = (field: keyof UserStory, index: number) => {
        const array = [...(formData[field] as string[])];
        array.splice(index, 1);
        setFormData({ ...formData, [field]: array });
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-hidden h-full w-full flex items-center justify-center z-50">
            <div className="relative bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 my-6">
                {/* Modal Header */}
                <div className="flex items-center justify-between px-6 py-4 border-b">
                    <h2 className="text-2xl font-bold text-gray-900">{story ? 'Edit' : 'Create'} User Story</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-500 focus:outline-none"
                        aria-label="Close"
                    >
                        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Modal Body */}
                <div className="px-6 py-4 max-h-[calc(100vh-16rem)] overflow-y-auto">
                    <form onSubmit={handleSubmit} id="userStoryForm" className="space-y-6">
                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Title</label>
                                <input
                                    type="text"
                                    name="title"
                                    value={formData.title}
                                    onChange={handleInputChange}
                                    className="w-full p-2 border rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                                    required
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                                <textarea
                                    name="description"
                                    value={formData.description}
                                    onChange={handleInputChange}
                                    className="w-full p-2 border rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 min-h-[100px]"
                                    required
                                />
                            </div>
                        </div>

                        {Object.entries(formData)
                            .filter(([key]) => Array.isArray(formData[key as keyof UserStory]))
                            .map(([key]) => {
                                const isFlowField = key === 'mainFlow' || key === 'alternativeFlows';
                                const textareaHeight = isFlowField ? 'min-h-[120px]' : 'min-h-[60px]';
                                const containerClass = isFlowField ? 'bg-gray-50 p-4 rounded-lg' : '';

                                return (
                                    <div key={key} className={`space-y-3 ${containerClass}`}>
                                        <label className="block text-sm font-medium text-gray-700 capitalize">
                                            {key.replace(/([A-Z])/g, ' $1').trim()}
                                        </label>
                                        <div className="space-y-3">
                                            {formData[key as keyof UserStory].map((item: string, index: number) => (
                                                <div key={index} className="flex gap-2 items-start">
                                                    <textarea
                                                        value={item}
                                                        onChange={(e) => handleListItemChange(key as keyof UserStory, index, e.target.value)}
                                                        className={`flex-1 p-2 border rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 ${textareaHeight}`}
                                                        placeholder={isFlowField ? `Step ${index + 1}` : ''}
                                                    />
                                                    <button
                                                        type="button"
                                                        onClick={() => handleRemoveListItem(key as keyof UserStory, index)}
                                                        className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-md transition-colors"
                                                        aria-label="Remove item"
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                            <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                                                        </svg>
                                                    </button>
                                                </div>
                                            ))}
                                            <button
                                                type="button"
                                                onClick={() => handleAddListItem(key as keyof UserStory)}
                                                className={`w-full px-4 py-2 text-sm ${
                                                    isFlowField 
                                                        ? 'bg-indigo-100 text-indigo-700 hover:bg-indigo-200'
                                                        : 'bg-gray-50 text-gray-600 hover:bg-gray-100'
                                                } rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2`}
                                            >
                                                Add {isFlowField ? `Step to ${key.replace(/([A-Z])/g, ' $1').trim()}` : key.replace(/([A-Z])/g, ' $1').trim()}
                                            </button>
                                        </div>
                                    </div>
                                );
                            })}
                    </form>
                </div>

                {/* Modal Footer */}
                <div className="px-6 py-4 bg-gray-50 border-t flex justify-end gap-4">
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-4 py-2 text-gray-700 hover:text-gray-900 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        form="userStoryForm"
                        className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                    >
                        {story ? 'Save Changes' : 'Create Story'}
                    </button>
                </div>
            </div>
        </div>
    );
}
