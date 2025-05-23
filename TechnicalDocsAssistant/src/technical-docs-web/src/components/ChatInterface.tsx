'use client';

import { useState, useRef, useEffect } from 'react';
import { Asset } from '../models/asset';
import { UserStory } from '@/services/userStoryService';
import ReactMarkdown from 'react-markdown';

const API_BASE_URL = 'http://localhost:5103/api';

interface ChatMessage {
    role: 'user' | 'assistant';
    content: string;
}

interface AssetReference {
    id: string;
    title: string;
    snippet: string;
    relevance: number;
}

interface ChatResponse {
    response: string;
    sources: AssetReference[];
}

interface ChatInterfaceProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function ChatInterface({ isOpen, onClose }: ChatInterfaceProps) {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [query, setQuery] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [sources, setSources] = useState<AssetReference[]>([]);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages]);

    const handleSubmit = async () => {
        if (!query.trim()) return;

        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}/chat/query`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    query,
                    history: messages
                })
            });

            if (!response.ok) {
                throw new Error('Failed to get response');
            }

            const data: ChatResponse = await response.json();
            
            setMessages(prev => [
                ...prev,
                { role: 'user', content: query },
                { role: 'assistant', content: data.response }
            ]);
            setSources(data.sources);
            setQuery('');
        } catch (error) {
            console.error('Error querying chat:', error);
            setMessages(prev => [
                ...prev,
                { role: 'user', content: query },
                { role: 'assistant', content: 'Sorry, I encountered an error processing your request.' }
            ]);
        } finally {
            setIsLoading(false);
        }
    };

    const renderUserStory = (content: string) => {
        try {
            const userStory = JSON.parse(content.replace('<div class="user-story-response">', '').replace('</div>', ''));
            return (
                <div className="bg-white rounded-lg shadow p-6 space-y-4">
                    <h2 className="text-xl font-semibold text-gray-900">{userStory.title}</h2>
                    <div className="space-y-4">
                        <div>
                            <h3 className="text-lg font-medium text-gray-900">Description</h3>
                            <p className="mt-1 text-gray-600">{userStory.description}</p>
                        </div>

                        {Object.entries(userStory).map(([key, value]) => {
                            if (key === 'title' || key === 'description') return null;
                            if (!Array.isArray(value) || value.length === 0) return null;

                            return (
                                <div key={key}>
                                    <h3 className="text-lg font-medium text-gray-900 capitalize">
                                        {key.replace(/([A-Z])/g, ' $1').trim()}
                                    </h3>
                                    <ul className="mt-2 list-disc list-inside space-y-1">
                                        {(value as string[]).map((item, index) => (
                                            <li key={index} className="text-gray-600">{item}</li>
                                        ))}
                                    </ul>
                                </div>
                            );
                        })}
                    </div>
                </div>
            );
        } catch (error) {
            return <div className="text-gray-600 whitespace-pre-wrap">{content}</div>;
        }
    };

    const renderMessage = (message: ChatMessage) => {
        const isUserStoryResponse = message.content.includes('<div class="user-story-response">');
        
        return (
            <div
                className={`flex ${
                    message.role === 'user' ? 'justify-end' : 'justify-start'
                }`}
            >
                <div
                    className={`max-w-3xl rounded-lg px-4 py-2 ${
                        message.role === 'user'
                            ? 'bg-indigo-500 text-white'
                            : 'bg-gray-100 text-gray-900'
                    }`}
                >
                    {isUserStoryResponse ? (
                        renderUserStory(message.content)
                    ) : (
                        <div className="text-sm prose prose-sm max-w-none">
                            <ReactMarkdown>{message.content}</ReactMarkdown>
                        </div>
                    )}
                </div>
            </div>
        );
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-11/12 max-w-4xl shadow-lg rounded-md bg-white">
                {/* Header */}
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-2xl font-bold text-gray-800">Chat with Your Documentation</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-500 hover:text-gray-700"
                    >
                        <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Messages Display */}
                <div className="h-96 overflow-y-auto mb-4 border rounded-lg bg-gray-50 p-4">
                    {messages.map((message, index) => (
                        <div
                            key={index}
                            className={`mb-4 ${
                                message.role === 'user' ? 'text-right' : 'text-left'
                            }`}
                        >
                            {renderMessage(message)}
                        </div>
                    ))}
                    <div ref={messagesEndRef} />
                </div>

                {/* Sources Display */}
                {sources && sources.length > 0 && (
                    <div className="mb-4 p-4 border rounded-lg bg-gray-50">
                        <h4 className="font-semibold text-gray-700 mb-2">Sources:</h4>
                        <div className="space-y-3">
                            {sources.map(source => (
                                <div key={source.id} className="bg-white p-3 rounded border">
                                    <h5 className="font-medium text-indigo-600">{source.title}</h5>
                                    <p className="text-sm text-gray-600 mt-1">{source.snippet}</p>
                                    <div className="text-xs text-gray-400 mt-1">
                                        Relevance: {Math.round(source.relevance * 100)}%
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {/* Input Area */}
                <div className="flex gap-2">
                    <input
                        type="text"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        onKeyPress={(e) => e.key === 'Enter' && handleSubmit()}
                        placeholder="Ask a question about your documentation..."
                        className="flex-1 p-2 border rounded focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                    />
                    <button
                        onClick={handleSubmit}
                        disabled={isLoading || !query.trim()}
                        className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isLoading ? 'Thinking...' : 'Send'}
                    </button>
                </div>
            </div>
        </div>
    );
}
