import { UserStory } from '@/services/userStoryService';

interface UserStoryListProps {
    userStories: UserStory[];
    selectedStory: UserStory | null;
    onSelect: (story: UserStory) => void;
}

export default function UserStoryList({ userStories, selectedStory, onSelect }: UserStoryListProps) {
    return (
        <div className="space-y-4">
            <h3 className="text-lg font-medium text-gray-900">Select a User Story</h3>
            <div className="grid gap-4">
                {userStories.map((story) => (
                    <button
                        key={story.id}
                        onClick={() => onSelect(story)}
                        className={`p-4 text-left border rounded-lg hover:bg-gray-50 transition-colors ${
                            selectedStory?.id === story.id ? 'border-indigo-500 bg-indigo-50' : 'border-gray-200'
                        }`}
                    >
                        <h4 className="font-medium text-gray-900">{story.title}</h4>
                        <p className="mt-1 text-sm text-gray-500">{story.description}</p>
                        <div className="mt-2 flex items-center text-xs text-gray-500">
                            <span>Created: {new Date(story.createdAt || '').toLocaleDateString()}</span>
                        </div>
                    </button>
                ))}
            </div>
        </div>
    );
}
