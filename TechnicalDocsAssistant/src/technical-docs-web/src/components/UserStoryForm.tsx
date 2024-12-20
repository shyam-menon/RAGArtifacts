'use client';

import { useState } from 'react';
import { UserStory } from '@/services/userStoryService';

interface UserStoryFormProps {
    onSubmit: (userStory: UserStory) => void;
    isLoading: boolean;
}

export default function UserStoryForm({ onSubmit, isLoading }: UserStoryFormProps) {
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
        <div key={field}>
            <label className="block text-sm font-medium text-gray-700 capitalize">
                {label}
            </label>
            {userStory[field].map((item: string, index: number) => (
                <div key={index} className="flex mt-1 space-x-2">
                    <input
                        type="text"
                        value={item}
                        onChange={(e) =>
                            handleArrayChange(field, index, e.target.value)
                        }
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                    />
                    <button
                        type="button"
                        onClick={() => removeArrayItem(field, index)}
                        className="px-2 py-1 text-sm text-red-600 hover:text-red-800"
                    >
                        Remove
                    </button>
                </div>
            ))}
            <button
                type="button"
                onClick={() => addArrayItem(field)}
                className="mt-2 text-sm text-indigo-600 hover:text-indigo-800"
            >
                Add {label.toLowerCase()}
            </button>
        </div>
    );

    return (
        <form onSubmit={handleSubmit} className="space-y-6">
            <div>
                <label className="block text-sm font-medium text-gray-700">Title</label>
                <input
                    type="text"
                    value={userStory.title}
                    onChange={(e) => setUserStory({ ...userStory, title: e.target.value })}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                    required
                />
            </div>

            <div>
                <label className="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                    value={userStory.description}
                    onChange={(e) => setUserStory({ ...userStory, description: e.target.value })}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                    rows={3}
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

            <div>
                <button
                    type="submit"
                    disabled={isLoading}
                    className={`w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white ${
                        isLoading
                            ? 'bg-indigo-400 cursor-not-allowed'
                            : 'bg-indigo-600 hover:bg-indigo-700'
                    }`}
                >
                    {isLoading ? 'Creating...' : 'Create User Story'}
                </button>
            </div>
        </form>
    );
}
