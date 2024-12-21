export interface Asset {
    id: string;
    title: string;
    markdownContent: string;
    contentVector: number[];
    created: string;
    modified: string;
    isDeleted: boolean;
}
