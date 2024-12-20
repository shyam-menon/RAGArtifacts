'use client';

import { TechnicalArtifact } from '@/types';
import { useEffect, useState } from 'react';

interface ArtifactDisplayProps {
    artifact: TechnicalArtifact | null;
}

export default function ArtifactDisplay({ artifact }: ArtifactDisplayProps) {
    const [plantUmlSvg, setPlantUmlSvg] = useState<string>('');

    useEffect(() => {
        if (artifact?.type === 'flowchart' || artifact?.type === 'sequence') {
            // In a real application, you would convert PlantUML to SVG here
            // For now, we'll just display the raw PlantUML
            setPlantUmlSvg('');
        }
    }, [artifact]);

    if (!artifact) {
        return null;
    }

    return (
        <div className="mt-6 bg-white shadow sm:rounded-lg">
            <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium leading-6 text-gray-900 capitalize">
                    {artifact.type} Output
                </h3>
                
                <div className="mt-4">
                    {artifact.type === 'testcases' ? (
                        <div 
                            className="prose max-w-none"
                            dangerouslySetInnerHTML={{ __html: artifact.content }}
                        />
                    ) : (
                        <div className="relative">
                            {plantUmlSvg ? (
                                <img 
                                    src={`data:image/svg+xml;base64,${plantUmlSvg}`}
                                    alt={`Generated ${artifact.type}`}
                                    className="w-full"
                                />
                            ) : (
                                <pre className="p-4 bg-gray-50 rounded-md overflow-auto">
                                    <code>{artifact.content}</code>
                                </pre>
                            )}
                        </div>
                    )}
                </div>

                <div className="mt-4 flex justify-end space-x-4">
                    <button
                        onClick={() => {
                            // Add download functionality
                            const blob = new Blob([artifact.content], { type: 'text/plain' });
                            const url = window.URL.createObjectURL(blob);
                            const a = document.createElement('a');
                            a.href = url;
                            a.download = `${artifact.type}-${artifact.id}.txt`;
                            document.body.appendChild(a);
                            a.click();
                            document.body.removeChild(a);
                            window.URL.revokeObjectURL(url);
                        }}
                        className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                    >
                        Download
                    </button>
                    <button
                        onClick={() => {
                            navigator.clipboard.writeText(artifact.content);
                        }}
                        className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-indigo-700 bg-indigo-100 hover:bg-indigo-200"
                    >
                        Copy to Clipboard
                    </button>
                </div>
            </div>
        </div>
    );
}
