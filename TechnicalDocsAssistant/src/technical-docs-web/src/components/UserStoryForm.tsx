'use client';

import { useState } from 'react';
import { UserStory } from '@/services/userStoryService';

interface UserStoryFormProps {
    onSubmit: (userStory: UserStory) => void;
    onCancel: () => void;
    isLoading: boolean;
}

export default function UserStoryForm({ onSubmit, onCancel, isLoading }: UserStoryFormProps) {
    const [userStory, setUserStory] = useState<UserStory>({
        title: '',
        description: '',
        actors: [''],
        preconditions: [''],
        postconditions: [''],
        mainFlow: [''],
        alternativeFlows: [''],
        businessRules: [''],
        dataRequirements: [''],
        nonFunctionalRequirements: [''],
        assumptions: ['']
    });

    const handleArrayChange = (
        field: keyof UserStory,
        index: number,
        value: string
    ) => {
        const array = [...(userStory[field] as string[])];
        array[index] = value;
        setUserStory({ ...userStory, [field]: array });
    };

    const addArrayItem = (field: keyof UserStory) => {
        setUserStory({
            ...userStory,
            [field]: [...(userStory[field] as string[]), '']
        });
    };

    const removeArrayItem = (field: keyof UserStory, index: number) => {
        const array = [...(userStory[field] as string[])];
        array.splice(index, 1);
        setUserStory({ ...userStory, [field]: array });
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSubmit(userStory);
    };

    const renderArrayField = (field: keyof UserStory, label: string) => (
        <div key={field} className="space-y-2">
            <label className="block text-sm font-medium text-gray-700 capitalize">
                {label}
            </label>
            <div className="space-y-2">
                {userStory[field].map((item: string, index: number) => (
                    <div key={index} className="flex items-center gap-2">
                        <input
                            type="text"
                            value={item}
                            onChange={(e) =>
                                handleArrayChange(field, index, e.target.value)
                            }
                            className="flex-1 rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-base p-2"
                        />
                        <button
                            type="button"
                            onClick={() => removeArrayItem(field, index)}
                            className="px-2 py-1 text-sm text-red-600 hover:text-red-800 hover:bg-red-50 rounded transition-colors"
                        >
                            Remove
                        </button>
                    </div>
                ))}
            </div>
            <button
                type="button"
                onClick={() => addArrayItem(field)}
                className="text-sm text-indigo-600 hover:text-indigo-800 hover:bg-indigo-50 px-3 py-1 rounded transition-colors"
            >
                Add {label.toLowerCase()}
            </button>
        </div>
    );

    return (
        <div className="w-full max-w-4xl mx-auto bg-white rounded-lg shadow">
            <form onSubmit={handleSubmit} className="divide-y divide-gray-200">
                {/* Form Fields Section */}
                <div className="p-6 space-y-6">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Title</label>
                        <input
                            type="text"
                            value={userStory.title}
                            onChange={(e) => setUserStory({ ...userStory, title: e.target.value })}
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-base p-2"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                        <textarea
                            value={userStory.description}
                            onChange={(e) => setUserStory({ ...userStory, description: e.target.value })}
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-base p-2"
                            rows={5}
                            required
                        />
                    </div>

                    {renderArrayField('actors', 'Actors')}
                    {renderArrayField('preconditions', 'Preconditions')}
                    {renderArrayField('postconditions', 'Postconditions')}
                    {renderArrayField('mainFlow', 'Main Flow Steps')}
                    {renderArrayField('alternativeFlows', 'Alternative Flows')}
                    {renderArrayField('businessRules', 'Business Rules')}
                    {renderArrayField('dataRequirements', 'Data Requirements')}
                    {renderArrayField('nonFunctionalRequirements', 'Non-Functional Requirements')}
                    {renderArrayField('assumptions', 'Assumptions')}
                </div>

                {/* Form Actions Section */}
                <div className="px-6 py-4 bg-gray-50 flex justify-end gap-4">
                    <button
                        type="button"
                        onClick={onCancel}
                        className="px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isLoading}
                        className={`flex justify-center py-2 px-6 border border-transparent rounded-md shadow-sm text-base font-medium text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 ${
                            isLoading
                                ? 'bg-indigo-400 cursor-not-allowed'
                                : 'bg-indigo-600 hover:bg-indigo-700'
                        }`}
                    >
                        {isLoading ? 'Creating...' : 'Create User Story'}
                    </button>
                </div>
            </form>
        </div>
    );
}
