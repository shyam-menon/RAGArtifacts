'use client';

import { TechnicalArtifact } from '@/types';
import { useState, useEffect } from 'react';
import plantumlEncoder from 'plantuml-encoder';

interface ArtifactDisplayProps {
    artifact: TechnicalArtifact | null;
}

export default function ArtifactDisplay({ artifact }: ArtifactDisplayProps) {
    const [imageUrl, setImageUrl] = useState<string | null>(null);

    useEffect(() => {
        if (artifact?.content && (artifact.type === 'flowchart' || artifact.type === 'sequence')) {
            try {
                // Get content without markdown markers
                let content = artifact.content.replace(/^```plantuml\n?|\n?```$/g, '');
                
                // Add PlantUML header if not present
                if (!content.startsWith('@startuml')) {
                    content = '@startuml\n' + content;
                }
                if (!content.endsWith('@enduml')) {
                    content += '\n@enduml';
                }

                // Use plantuml-encoder to generate the URL
                const encoded = plantumlEncoder.encode(content);
                const url = `http://www.plantuml.com/plantuml/svg/${encoded}`;
                setImageUrl(url);
            } catch (error) {
                console.error('Error generating PlantUML URL:', error);
                setImageUrl(null);
            }
        } else {
            setImageUrl(null);
        }
    }, [artifact]);

    if (!artifact) {
        return null;
    }

    const handleCopyClick = () => {
        // For test cases, copy the raw content without HTML tags
        if (artifact.type === 'testcases') {
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = artifact.content;
            navigator.clipboard.writeText(tempDiv.textContent || '');
        } else {
            // Copy the content as is, preserving PlantUML syntax
            navigator.clipboard.writeText(artifact.content);
        }
    };

    const renderContent = () => {
        if (artifact.type === 'testcases') {
            return (
                <div 
                    className="prose max-w-none" 
                    dangerouslySetInnerHTML={{ __html: artifact.content }}
                />
            );
        }

        // Display the content as is, preserving PlantUML syntax
        return (
            <pre className="mt-4 bg-gray-50 rounded-lg p-4 overflow-x-auto">
                <code className="text-sm text-gray-800">{artifact.content}</code>
            </pre>
        );
    };

    return (
        <div className="mt-8">
            <div className="bg-white shadow sm:rounded-lg">
                <div className="px-4 py-5 sm:p-6">
                    <h3 className="text-lg font-medium leading-6 text-gray-900 mb-4">
                        {artifact.type.charAt(0).toUpperCase() + artifact.type.slice(1)} Output
                    </h3>
                    
                    {/* PlantUML Diagram */}
                    {imageUrl && (
                        <div className="mb-4 flex justify-center">
                            <img 
                                src={imageUrl} 
                                alt={`${artifact.type} diagram`}
                                className="max-w-full h-auto border border-gray-200 rounded-lg"
                                onError={(e) => {
                                    console.error('Error loading diagram');
                                    setImageUrl(null);
                                }}
                            />
                        </div>
                    )}

                    {/* Content */}
                    <div className="relative">
                        {renderContent()}
                        <button
                            onClick={handleCopyClick}
                            className="absolute top-2 right-2 px-3 py-1 text-sm text-white bg-indigo-600 rounded hover:bg-indigo-700 transition-colors"
                        >
                            Copy
                        </button>
                    </div>

                    <div className="mt-4 flex justify-end space-x-4">
                        <button
                            onClick={() => {
                                let content = artifact.content;
                                if (artifact.type === 'testcases') {
                                    const tempDiv = document.createElement('div');
                                    tempDiv.innerHTML = content;
                                    content = tempDiv.textContent || '';
                                }
                                const blob = new Blob([content], { type: 'text/plain' });
                                const url = URL.createObjectURL(blob);
                                const a = document.createElement('a');
                                a.href = url;
                                a.download = `${artifact.type}-${new Date().toISOString()}.txt`;
                                document.body.appendChild(a);
                                a.click();
                                document.body.removeChild(a);
                                URL.revokeObjectURL(url);
                            }}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700"
                        >
                            Download
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
