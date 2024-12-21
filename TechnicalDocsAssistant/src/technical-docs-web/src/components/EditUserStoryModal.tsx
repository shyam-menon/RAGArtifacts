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

    const handleArrayChange = (field: keyof UserStory, index: number, value: string) => {
        const array = [...(formData[field] as string[])];
        array[index] = value;
        setFormData({ ...formData, [field]: array });
    };

    const addArrayItem = (field: keyof UserStory) => {
        setFormData({
            ...formData,
            [field]: [...(formData[field] as string[]), '']
        });
    };

    const removeArrayItem = (field: keyof UserStory, index: number) => {
        const array = [...(formData[field] as string[])];
        array.splice(index, 1);
        setFormData({ ...formData, [field]: array });
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-[800px] shadow-lg rounded-md bg-white">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-medium">Edit User Story</h3>
                    <button
                        onClick={onClose}
                        className="text-gray-500 hover:text-gray-700"
                    >
                        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
                
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700">Title</label>
                        <input
                            type="text"
                            value={formData.title}
                            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700">Description</label>
                        <textarea
                            value={formData.description}
                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                            rows={3}
                            required
                        />
                    </div>

                    {(['actors', 'preconditions', 'postconditions', 'mainFlow', 'alternativeFlows', 'businessRules', 'dataRequirements', 'nonFunctionalRequirements', 'assumptions'] as const).map((field) => (
                        <div key={field}>
                            <label className="block text-sm font-medium text-gray-700 capitalize">
                                {field.replace(/([A-Z])/g, ' $1').trim()}
                            </label>
                            <div className="space-y-2">
                                {formData[field].map((item: string, index: number) => (
                                    <div key={index} className="flex gap-2">
                                        <input
                                            type="text"
                                            value={item}
                                            onChange={(e) => handleArrayChange(field, index, e.target.value)}
                                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                                        />
                                        <button
                                            type="button"
                                            onClick={() => removeArrayItem(field, index)}
                                            className="text-red-500 hover:text-red-700"
                                        >
                                            <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                                            </svg>
                                        </button>
                                    </div>
                                ))}
                                <button
                                    type="button"
                                    onClick={() => addArrayItem(field)}
                                    className="mt-2 inline-flex items-center px-3 py-1 border border-transparent text-sm leading-4 font-medium rounded-md text-indigo-700 bg-indigo-100 hover:bg-indigo-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                                >
                                    Add {field.replace(/([A-Z])/g, ' $1').trim().toLowerCase()}
                                </button>
                            </div>
                        </div>
                    ))}

                    <div className="flex justify-end gap-2 pt-4">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                        >
                            Save Changes
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
