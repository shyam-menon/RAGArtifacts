import { UserStory } from '@/services/userStoryService';
import { useState, useEffect } from 'react';

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
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full flex items-center justify-center">
            <div className="relative bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                <div className="p-6">
                    <h2 className="text-2xl font-bold mb-6">{story ? 'Edit' : 'Create'} User Story</h2>
                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Title</label>
                            <input
                                type="text"
                                name="title"
                                value={formData.title}
                                onChange={handleInputChange}
                                className="w-full p-2 border rounded-md focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                            <textarea
                                name="description"
                                value={formData.description}
                                onChange={handleInputChange}
                                className="w-full p-2 border rounded-md focus:ring-2 focus:ring-indigo-500 min-h-[100px]"
                                required
                            />
                        </div>

                        {Object.entries(formData)
                            .filter(([key]) => Array.isArray(formData[key as keyof UserStory]))
                            .map(([key]) => {
                                // Determine if this is a flow field that needs larger height
                                const isFlowField = key === 'mainFlow' || key === 'alternativeFlows';
                                const textareaHeight = isFlowField ? 'min-h-[120px]' : 'min-h-[60px]';
                                const containerClass = isFlowField ? 'bg-gray-50 p-4 rounded-lg' : '';

                                return (
                                    <div key={key} className={`space-y-2 ${containerClass}`}>
                                        <label className="block text-sm font-medium text-gray-700 capitalize">
                                            {key.replace(/([A-Z])/g, ' $1').trim()}
                                        </label>
                                        <div className="space-y-3">
                                            {formData[key as keyof UserStory].map((item: string, index: number) => (
                                                <div key={index} className="flex gap-2">
                                                    <textarea
                                                        value={item}
                                                        onChange={(e) => handleListItemChange(key as keyof UserStory, index, e.target.value)}
                                                        className={`flex-1 p-2 border rounded-md focus:ring-2 focus:ring-indigo-500 ${textareaHeight}`}
                                                        placeholder={isFlowField ? `Step ${index + 1}` : ''}
                                                    />
                                                    <button
                                                        type="button"
                                                        onClick={() => handleRemoveListItem(key as keyof UserStory, index)}
                                                        className="px-2 py-1 text-red-600 hover:text-red-800"
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
                                                } rounded-md transition-colors`}
                                            >
                                                Add {isFlowField ? `Step to ${key.replace(/([A-Z])/g, ' $1').trim()}` : key.replace(/([A-Z])/g, ' $1').trim()}
                                            </button>
                                        </div>
                                    </div>
                                );
                            })}
                        <div className="flex justify-end gap-4 pt-4 border-t">
                            <button
                                type="button"
                                onClick={onClose}
                                className="px-4 py-2 text-gray-600 hover:text-gray-800"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                            >
                                {story ? 'Save Changes' : 'Create Story'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}
