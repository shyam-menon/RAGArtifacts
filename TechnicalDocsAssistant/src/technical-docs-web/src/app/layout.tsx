import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
    title: 'Technical Documentation Assistant',
    description: 'Generate technical documentation artifacts from user stories',
};

export default function RootLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <html lang="en" className="h-full bg-gray-50">
            <body className={`${inter.className} h-full`}>{children}</body>
        </html>
    );
}
