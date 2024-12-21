'use client';

import { useState, useEffect } from 'react';
import { Asset } from '../models/asset';

const API_BASE_URL = 'http://localhost:5103/api';

interface AssetManagementProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function AssetManagement({ isOpen, onClose }: AssetManagementProps) {
    const [assets, setAssets] = useState<Asset[]>([]);
    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');
    const [searchQuery, setSearchQuery] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (isOpen) {
            loadAssets();
        }
    }, [isOpen]);

    const loadAssets = async () => {
        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}/assets`);
            const data = await response.json();
            console.log('Asset data:', data); // Debug log
            setAssets(data);
        } catch (error) {
            console.error('Error loading assets:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleCreateAsset = async () => {
        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}/assets`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ title, markdownContent: content }),
            });
            if (response.ok) {
                setTitle('');
                setContent('');
                await loadAssets();
            }
        } catch (error) {
            console.error('Error creating asset:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleDeleteAsset = async (id: string) => {
        if (!confirm('Are you sure you want to delete this asset?')) return;
        
        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}/assets/${id}`, {
                method: 'DELETE',
            });
            if (response.ok) {
                await loadAssets();
            } else {
                console.error('Failed to delete asset:', await response.text());
            }
        } catch (error) {
            console.error('Error deleting asset:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleSearch = async () => {
        try {
            setIsLoading(true);
            const response = await fetch(`${API_BASE_URL}/assets/search?query=${encodeURIComponent(searchQuery)}`);
            const data = await response.json();
            setAssets(data);
        } catch (error) {
            console.error('Error searching assets:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const formatDate = (dateString: string) => {
        try {
            const date = new Date(dateString);
            return date instanceof Date && !isNaN(date.getTime()) 
                ? date.toLocaleDateString('en-US', { 
                    year: 'numeric', 
                    month: 'short', 
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                  })
                : 'Date not available';
        } catch {
            return 'Date not available';
        }
    };

    if (!isOpen) return null;

    return (
        <div key="asset-management-modal" className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-11/12 max-w-4xl shadow-lg rounded-md bg-white">
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-2xl font-bold text-gray-800">Asset Management</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-500 hover:text-gray-700"
                    >
                        <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Create Asset Form */}
                <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                    <h3 className="text-lg font-semibold mb-3">Create New Asset</h3>
                    <div className="space-y-3">
                        <input
                            type="text"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            placeholder="Asset Title"
                            className="w-full p-2 border rounded"
                        />
                        <textarea
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            placeholder="Markdown Content"
                            rows={4}
                            className="w-full p-2 border rounded"
                        />
                        <button
                            onClick={handleCreateAsset}
                            disabled={isLoading || !title || !content}
                            className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 disabled:opacity-50"
                        >
                            Create Asset
                        </button>
                    </div>
                </div>

                {/* Search Assets */}
                <div className="mb-6">
                    <div className="flex gap-2">
                        <input
                            type="text"
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            placeholder="Search assets..."
                            className="flex-1 p-2 border rounded"
                        />
                        <button
                            onClick={handleSearch}
                            disabled={isLoading || !searchQuery}
                            className="bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700 disabled:opacity-50"
                        >
                            Search
                        </button>
                        <button
                            onClick={loadAssets}
                            disabled={isLoading}
                            className="bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700 disabled:opacity-50"
                        >
                            Reset
                        </button>
                    </div>
                </div>

                {/* Assets List */}
                <div className="space-y-4">
                    {isLoading ? (
                        <div className="text-center py-4">Loading...</div>
                    ) : assets.length === 0 ? (
                        <div className="text-center py-4 text-gray-500">No assets found</div>
                    ) : (
                        assets.map((asset) => (
                            <div key={asset.id} className="border p-4 rounded-lg">
                                <div className="flex justify-between items-start">
                                    <div className="flex-1">
                                        <div className="flex items-baseline gap-2 mb-2">
                                            <h4 className="font-semibold text-lg">{asset.Title}</h4>
                                            <span className="text-sm text-gray-600">
                                                Last managed: {formatDate(asset.Modified)}
                                            </span>
                                        </div>
                                        <div className="mt-2 prose prose-sm max-w-none">
                                            {asset.MarkdownContent}
                                        </div>
                                    </div>
                                    <button
                                        onClick={() => handleDeleteAsset(asset.Id)}
                                        className="text-red-600 hover:text-red-800"
                                    >
                                        <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                        </svg>
                                    </button>
                                </div>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}
